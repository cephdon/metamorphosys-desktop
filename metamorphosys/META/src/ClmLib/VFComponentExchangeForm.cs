﻿/*
Copyright (C) 2013-2015 MetaMorph Software, Inc

Permission is hereby granted, free of charge, to any person obtaining a
copy of this data, including any software or models in source or binary
form, as well as any drawings, specifications, and documentation
(collectively "the Data"), to deal in the Data without restriction,
including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Data, and to
permit persons to whom the Data is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Data.

THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  

=======================
This version of the META tools is a fork of an original version produced
by Vanderbilt University's Institute for Software Integrated Systems (ISIS).
Their license statement:

Copyright (C) 2011-2014 Vanderbilt University

Developed with the sponsorship of the Defense Advanced Research Projects
Agency (DARPA) and delivered to the U.S. Government with Unlimited Rights
as defined in DFARS 252.227-7013.

Permission is hereby granted, free of charge, to any person obtaining a
copy of this data, including any software or models in source or binary
form, as well as any drawings, specifications, and documentation
(collectively "the Data"), to deal in the Data without restriction,
including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Data, and to
permit persons to whom the Data is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Data.

THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GME.MGA;
using ISIS.VehicleForge;
using ISIS.Web;

namespace ClmLib
{
    public partial class VFComponentExchangeForm : Form
    {
        private List<ClmLib.VFComponentListItem> dataSource;

        private VFExchange exchange;
        private VFExchangeFilter filter;
        private string zipDownloadDir;

        private IComponentLibrarySearchResult<VFComponent> searchResult { get; set; }

        public List<string> DownloadedComponentZips { get; set; }

        public VFComponentExchangeForm(VFExchange exchange, VFExchangeFilter filter, string zipDownloadDir)
        {
            this.InitializeComponent();

            this.exchange = exchange;
            this.filter = filter;
            this.zipDownloadDir = zipDownloadDir;
            this.dataSource = new List<ClmLib.VFComponentListItem>();
            this.DownloadedComponentZips = new List<string>();

            // Show which Component Category we are searching for
            this.lblComponentCategory.Text = this.filter.Category;

            //try
            //{
            this.searchResult = this.exchange.Search(this.filter);

            if (this.searchResult == null)
            {
                throw new VFCategoryNotFoundException();
            }

            int highestDisplayedIndex = (this.filter.NumberOfResults > this.searchResult.hits) ? 
                this.searchResult.hits : 
                this.filter.NumberOfResults;

            this.lblSearchResultsInfo.Text =
                string.Format(
                "Showing {0}-{1} of {2} search hits",
                this.filter.StartPosition + 1,
                highestDisplayedIndex,
                //this.filter.StartPosition + this.filter.NumberOfResults,
                this.searchResult.hits
                );

            foreach (var comp in searchResult.components)
            {
                this.dataSource.Add(new ClmLib.VFComponentListItem(comp));
            }

            this.dgvSelectorVFComponents.DataSource = this.dataSource;
            this.dgvSelectorVFComponents.ClearSelection();
            //}
            //catch (VFInvalidURLException)
            //{
            //    MessageBox.Show("Invalid VehicleForge URL");
            //}
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Need to update the filter to show the previous set/page of components

            // Need to Search VF again

            //this.searchResult = this.exchange.Search(this.filter);

            //this.lblSearchResultsInfo.Text =
            //    string.Format(
            //    "Showing {0}-{1} of {2} search hits",
            //    this.filter.StartPosition,
            //    this.filter.StartPosition + this.filter.NumberOfResults,
            //    this.searchResult.hits);

            //foreach (var comp in searchResult.components)
            //{
            //    this.dataSource.Add(new ClmLib.VFComponentListItem(comp));
            //}

            //this.dgvSelectorVFComponents.DataSource = this.dataSource;
            //this.dgvSelectorVFComponents.ClearSelection();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Need to update the filter to show the previous set/page of components

            // Need to Search VF again

            //this.searchResult = this.exchange.Search(this.filter);

            //this.lblSearchResultsInfo.Text =
            //    string.Format(
            //    "Showing {0}-{1} of {2} search hits",
            //    this.filter.StartPosition,
            //    this.filter.StartPosition + this.filter.NumberOfResults,
            //    this.searchResult.hits);

            //foreach (var comp in searchResult.components)
            //{
            //    this.dataSource.Add(new ClmLib.VFComponentListItem(comp));
            //}

            //this.dgvSelectorVFComponents.DataSource = this.dataSource;
            //this.dgvSelectorVFComponents.ClearSelection();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (this.dgvSelectorVFComponents.SelectedRows.Count == 0)
            {
                var msgBoxResponse = MessageBox.Show(
                    "No Components have been selected for Import. VehicleForge Component Exchange Dialog will close.",
                    "Nothing Happened",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (msgBoxResponse == System.Windows.Forms.DialogResult.OK)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    this.Close();
                }
                else
                {
                    return;
                }
            }

            List<string> componentZipPaths = new List<string>();

            using (VFDownloadProgressDialog downloadProgressDialog = new VFDownloadProgressDialog())
            {
                // set the size of the download progess bar, and print how many components will be downloaded
                downloadProgressDialog.pbDownloadProgress.Maximum = this.dgvSelectorVFComponents.SelectedRows.Count;
                downloadProgressDialog.lblNumberComponents.Text = string.Format("{0} Components", this.dgvSelectorVFComponents.SelectedRows.Count);
                downloadProgressDialog.Show();

                foreach (var row in this.dgvSelectorVFComponents.SelectedRows)
                {
                    var dgvr = row as System.Windows.Forms.DataGridViewRow;
                    var vfcli = dgvr.DataBoundItem as VFComponentListItem;

                    try
                    {
                        string newZipPath = exchange.DownloadComponent(vfcli.Component, this.zipDownloadDir);

                        if (newZipPath != null)
                        {
                            componentZipPaths.Add(newZipPath);
                        }

                        downloadProgressDialog.pbDownloadProgress.PerformStep();
                        downloadProgressDialog.pbDownloadProgress.Refresh();

                    }
                    catch (ISIS.VehicleForge.VFDownloadFailedException ex)
                    {
                        var msgBoxResponse = MessageBox.Show(
                            "Unable to download component.",
                            "Component download failed.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }

            this.DownloadedComponentZips.AddRange(componentZipPaths);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private bool sortByNameAscending = true;
        private bool sortByItarAscending = true;
        private bool sortByStatusAscending = true;

        private void dgvSelectorVFComponents_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvSelectorVFComponents.DataSource = null;

            if (this.dgvSelectorVFComponents.Columns[e.ColumnIndex].HeaderText == "Name")
            {
                if (sortByNameAscending)
                {
                    dataSource = dataSource.OrderBy(x => x.Name).ToList();
                    sortByNameAscending = !sortByNameAscending;
                }
                else
                {
                    dataSource = dataSource.OrderByDescending(x => x.Name).ToList();
                    sortByNameAscending = !sortByNameAscending;
                }
                //dataSource.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            else if (this.dgvSelectorVFComponents.Columns[e.ColumnIndex].HeaderText == "ItarRestriction")
            {
                if (sortByItarAscending)
                {
                    dataSource = dataSource.OrderBy(x => x.ItarRestriction).ToList();
                    sortByItarAscending = !sortByItarAscending;
                }
                else
                {
                    dataSource = dataSource.OrderByDescending(x => x.ItarRestriction).ToList();
                    sortByItarAscending = !sortByItarAscending;
                }
                //dataSource.Sort((x, y) => x.ItarRestriction.CompareTo(y.ItarRestriction));
            }
            else if (this.dgvSelectorVFComponents.Columns[e.ColumnIndex].HeaderText == "Status")
            {
                if (sortByStatusAscending)
                {
                    dataSource = dataSource.OrderBy(x => x.Status).ToList();
                    sortByStatusAscending = !sortByStatusAscending;
                }
                else
                {
                    dataSource = dataSource.OrderByDescending(x => x.Status).ToList();
                    sortByStatusAscending = !sortByStatusAscending;
                }
                //dataSource.Sort((x, y) => x.Status.CompareTo(y.Status));
            }

            this.dgvSelectorVFComponents.DataSource = dataSource;
            this.dgvSelectorVFComponents.Refresh();
        }

    }
}