

#ifndef _WIN32
#include <linux/types.h>
#else
#include <stdio.h>
#include <io.h>
#define lseek _lseek
#endif

#include "module.h"
std::vector<gbsim_cport> cport_list;

/*
 * Greybus kernel assigns hd-cport-ids to cports in the order they are present
 * in manifest. To match that here, we can just use a simple counter.
 */
static uint16_t hd_cport_id_counter;
static int control_done;

static uint16_t allocate_hd_cport_id(void)
{
    /*
	 * AP's hd_cport_id GB_SVC_CPORT_ID is reserved and must not be used for
	 * other protocols.
	 */
    if (hd_cport_id_counter == GB_SVC_CPORT_ID)
        ++hd_cport_id_counter;

    return hd_cport_id_counter++;
}

void reset_hd_cport_id(void)
{
    hd_cport_id_counter = 0;
}

static struct greybus_manifest_header* get_manifest_blob(const char* mnfs)
{
    struct greybus_manifest_header* mh;
    int mnf_fd, n;
    uint16_t size;

    if (!(mh = (struct greybus_manifest_header*)malloc(64 * 1024))) {
        printf("failed to allocate manifest buffer\n");
        return NULL;
    }

    if ((mnf_fd = open(mnfs, O_RDONLY)) < 0) {
        printf("failed to open manifest blob %s\n", mnfs);
        goto out;
    }

    if ((n = read(mnf_fd, &size, 2)) != 2) {
        printf("failed to read manifest size, read %d\n", n);
        goto out;
    }
    lseek(mnf_fd, 0, SEEK_SET);

    if (read(mnf_fd, mh, size) != size) {
        printf("failed to read manifest\n");
        goto out;
    }

    return mh;
out:
    free(mh);
    return NULL;
}
/*
 * Validate the given descriptor.  Its reported size must fit within
 * the number of bytes reamining, and it must have a recognized
 * type.  Check that the reported size is at least as big as what
 * we expect to see.  (It could be bigger, perhaps for a new version
 * of the format.)
 *
 * Returns the number of bytes consumed by the descriptor, or a
 * negative errno.
 */
void allocate_cport(uint16_t cport_id, uint16_t hd_cport_id, int protocol_id)
{

    struct gbsim_cport* cport;

    cport = (struct gbsim_cport*)malloc(sizeof(*cport));
    cport->id = cport_id;

    cport->hd_cport_id = hd_cport_id;
    cport->protocol = protocol_id;
    cport_list.push_back(*cport);
}

static int identify_descriptor(struct greybus_descriptor* desc, size_t size)
{
    struct greybus_descriptor_header* desc_header = &desc->header;
    size_t expected_size;
    size_t desc_size;

    if (size < sizeof(*desc_header)) {
        printf("manifest too small\n");
        return -EINVAL; /* Must at least have header */
    }

    desc_size = (int)le16toh(desc_header->size);
    if ((size_t)desc_size > size) {
        printf("descriptor too big\n");
        return -EINVAL;
    }

    /* Descriptor needs to at least have a header */
    expected_size = sizeof(*desc_header);

    switch (desc_header->type) {
    case GREYBUS_TYPE_STRING:
        expected_size += sizeof(struct greybus_descriptor_string);
        expected_size += desc->string.length;

        /* String descriptors are padded to 4 byte boundaries */
        expected_size = ALIGN(expected_size);
        break;
    case GREYBUS_TYPE_INTERFACE:
        expected_size += sizeof(struct greybus_descriptor_interface);
        break;
    case GREYBUS_TYPE_BUNDLE:
        expected_size += sizeof(struct greybus_descriptor_bundle);
        break;
    case GREYBUS_TYPE_CPORT:
        expected_size += sizeof(struct greybus_descriptor_cport);

        /*
		 * Module's control protocol's node might not be present in
		 * manifest, and the first allocated cport should be for control
		 * protocol.
		 */
        if (!control_done && (le16toh(desc->cport.id) != GB_CONTROL_CPORT_ID)) {
            allocate_cport(GB_CONTROL_CPORT_ID,
                allocate_hd_cport_id(),
                GREYBUS_PROTOCOL_CONTROL);
        }

        control_done = 1;
        allocate_cport(le16toh(desc->cport.id), allocate_hd_cport_id(),
            desc->cport.protocol_id);
        break;
    case GREYBUS_TYPE_INVALID:
    default:
        printf("invalid descriptor type (%hhu)\n", desc_header->type);
        return -EINVAL;
    }

    if (desc_size < expected_size) {
        printf("%d descriptor too small (%zu < %zu)\n",
            desc_header->type, desc_size, expected_size);
        return -EINVAL;
    }

    /* Warn if there is a size mismatch */
    if (desc_size != expected_size) {
        printf("%d descriptor size mismatch, expected - %zu, actual - %zu)\n",
            desc_header->type, expected_size, desc_size);
    }

    return desc_size;
}

/*
 * Parse a buffer containing a Interface manifest.
 *
 * If we find anything wrong with the content/format of the buffer
 * we reject it.
 *
 * The first requirement is that the manifest's version is
 * one we can parse.
 *
 * We make an initial pass through the buffer and identify all of
 * the descriptors it contains, keeping track for each its type
 * and the location size of its data in the buffer.
 *
 * Next we scan the descriptors, looking for a interface descriptor;
 * there must be exactly one of those.  When found, we record the
 * information it contains, and then remove that descriptor (and any
 * string descriptors it refers to) from further consideration.
 *
 * After that we look for the interface's bundles--there must be at
 * least one of those.
 *
 * Returns true if parsing was successful, false otherwise.
 */
bool manifest_parse(void* data, size_t size)
{
    struct greybus_manifest* manifest;
    struct greybus_manifest_header* header;
    struct greybus_descriptor* desc;
    __u16 manifest_size;

    /* we have to have at _least_ the manifest header */
    if (size <= sizeof(manifest->header)) {
        printf("short manifest (%zu)\n", size);
        return false;
    }

    /* Make sure the size is right */
    manifest = (struct greybus_manifest*)data;
    header = &manifest->header;
    manifest_size = le16toh(header->size);
    if (manifest_size != size) {
        printf("manifest size mismatch %zu != %hu\n",
            size, manifest_size);
        return false;
    }

    /* Validate major/minor number */
    if (header->version_major > GREYBUS_VERSION_MAJOR) {
        printf("manifest version too new (%hhu.%hhu > %hhu.%hhu)\n",
            header->version_major, header->version_minor,
            GREYBUS_VERSION_MAJOR, GREYBUS_VERSION_MINOR);
        return false;
    }

    /* OK, find all the descriptors */
    desc = (struct greybus_descriptor*)(header + 1);
    size -= sizeof(*header);

    /* Reset control protocol's counter */
    control_done = 0;

    while (size) {
        int desc_size;

        desc_size = identify_descriptor(desc, size);
        if (desc_size < 0)
            return false;

        desc = (struct greybus_descriptor*)((char*)desc + desc_size);
        size -= desc_size;
    }

    return true;
}