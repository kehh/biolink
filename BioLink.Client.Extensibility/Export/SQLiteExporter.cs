﻿/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility.Export {
    public class SQLiteExporter : TabularDataExporter {

        protected override object GetOptions(Window parentWindow, DataMatrix matrix, String datasetName) {

            string filename = PromptForFilename(".sqlite", "SQLite database (.sqlite)|*.sqlite", datasetName);
            if (!String.IsNullOrEmpty(filename)) {
                ExcelExporterOptions options = new ExcelExporterOptions();
                options.Filename = filename;
                return options;
            }

            return null;
        }

        public override void ExportImpl(Window parentWindow, Data.DataMatrix matrix, string datasetName, object optionsObj) {
            ExcelExporterOptions options = optionsObj as ExcelExporterOptions;

            if (options == null) {
                return;
            }

            ProgressStart("Exporting...");

            int totalRows = matrix.Rows.Count;

            if (FileExistsAndNotOverwrite(options.Filename)) {
                return;
            }
            FileInfo file = new FileInfo(options.Filename);

            using (SQLiteExporterService service = new SQLiteExporterService(file.FullName)) {
                service.CreateTable("ExportedData", matrix);

                service.BeginTransaction();
                int count = 0;
                foreach (MatrixRow row in matrix) {
                    service.InsertRow("ExportedData", row);
                    if (++count % 1000 == 0) {
                        double percent = ((double)count / (double)totalRows) * 100.0;
                        ProgressMessage(String.Format("Exporting {0} rows of {1}...", count, totalRows), percent);
                    }
                }
                ProgressMessage(String.Format("Saving...", count, totalRows));
                service.CommitTransaction();
            }

            ProgressEnd(String.Format("{0} rows exported.", totalRows));
        }

        public override void Dispose() {
        }

        #region Properties

        public override string Description {
            get { return "Export data as a SQLite database file"; }
        }

        public override string Name {
            get { return "SQLite database"; }
        }

        public override BitmapSource Icon {
            get {
                return ImageCache.GetPackedImage("images/sqlite_exporter.png", GetType().Assembly.GetName().Name);
            }
        }

        #endregion

        public override bool CanExport(DataMatrix matrix, String datasetName) {
            return true;
        }
    }

    internal class SQLiteExporterService : SQLiteServiceBase {

        public SQLiteExporterService(string filename)
            : base(filename, true) {
        }

        public void CreateTable(string tableName, DataMatrix matrix) {

            StringBuilder columnDefs = new StringBuilder();
            int numCols = matrix.Columns.Count;
            foreach (MatrixColumn col in matrix.Columns) {
                if (!col.IsHidden) {
                    columnDefs.AppendFormat("[{0}]", col.Name).Append(" TEXT");
                    columnDefs.Append(", ");
                }
            }

            columnDefs.Remove(columnDefs.Length - 2, 2);
            
            Command((cmd) => {
                cmd.CommandText = String.Format("CREATE TABLE [{0}] ({1})", tableName, columnDefs.ToString());
                cmd.ExecuteNonQuery();
            });

        }

        public void InsertRow(string tableName, MatrixRow row) {

            int numCols = row.Matrix.Columns.Count;            
            StringBuilder b = new StringBuilder();
            
            foreach (MatrixColumn col in row.Matrix.Columns) {
                if (!col.IsHidden) {
                    b.Append("@").Append(col.Name.Replace(" ", ""));
                    b.Append(", ");
                }
            }

            b.Remove(b.Length - 2, 2);
            
            Command((cmd) => {                
                cmd.CommandText = String.Format(@"INSERT INTO [{0}] VALUES ({1})", tableName, b.ToString());
                int currentCol = 0;
                foreach (MatrixColumn col in row.Matrix.Columns) {
                    if (!col.IsHidden) {
                        cmd.Parameters.Add(new SQLiteParameter("@" + col.Name.Replace(" ", ""), row[currentCol++]));
                    }
                }
                cmd.ExecuteNonQuery();
            });

        }

    }
}
