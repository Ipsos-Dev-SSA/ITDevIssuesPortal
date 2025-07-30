
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.IO;
//using System.Data;
//using System.Data.SqlClient;
//using System.Configuration;
//using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
//using CallCentreFollowUps.Models;

//namespace CallCentreFollowUps.Controllers
//{
//    public class AdminController : Controller
//    {
//        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CallCentreDB"].ConnectionString;

//        public ActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult ImportExcel()
//        {
//            try
//            {
//                if (Request.Files.Count == 0)
//                {
//                    TempData["Error"] = "No file uploaded.";
//                    return RedirectToAction("Index");
//                }

//                HttpPostedFileBase file = Request.Files[0];
//                string fileName = Path.GetFileName(file.FileName);
//                string path = Server.MapPath("~/FilesUploaded/");

//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }

//                string filePath = Path.Combine(path, fileName);
//                file.SaveAs(filePath);

//                DataTable dt = ReadExcelToDataTable(filePath);
//                ClearStagingTable();
//                BulkInsertIntoStaging(dt);
//                RemoveDuplicatesInStaging();
//                RemoveExistingRespondents();
//                MoveDataToRespondent();

//                TempData["Success"] = "File uploaded and processed successfully.";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Error processing file: " + ex.Message;
//            }

//            return RedirectToAction("Index");
//        }
//        private void BulkInsertIntoStaging(DataTable dt)
//        {
//            using (SqlConnection con = new SqlConnection(_connectionString))
//            {
//                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
//                {
//                    bulkCopy.DestinationTableName = "tblStaging";

//                    bulkCopy.ColumnMappings.Add("id", "id");
//                    bulkCopy.ColumnMappings.Add("CellDescription", "CellDescription");
//                    bulkCopy.ColumnMappings.Add("technicalid", "technicalid");
//                    bulkCopy.ColumnMappings.Add("cellid", "cellid");
//                    bulkCopy.ColumnMappings.Add("celltarget", "celltarget");
//                    bulkCopy.ColumnMappings.Add("cellpreference", "cellpreference");

//                    con.Open();
//                    bulkCopy.WriteToServer(dt);
//                }
//            }
//        }
//        private void ClearStagingTable()
//        {
//            using (SqlConnection con = new SqlConnection(_connectionString))
//            {
//                con.Open();
//                SqlCommand cmd = new SqlCommand("DELETE FROM tblStaging", con);
//                cmd.ExecuteNonQuery();
//            }
//        }

//        private void RemoveDuplicatesInStaging()
//        {
//            using (SqlConnection con = new SqlConnection(_connectionString))
//            {
//                con.Open();
//                SqlCommand cmd = new SqlCommand(@"
//                    WITH CTE_Duplicates AS (
//                        SELECT *, ROW_NUMBER() OVER (PARTITION BY id ORDER BY Id) AS row_num 
//                        FROM tblStaging
//                    )
//                    DELETE FROM CTE_Duplicates WHERE row_num > 1", con);
//                cmd.ExecuteNonQuery();
//            }
//        }

//        private void RemoveExistingRespondents()
//        {
//            using (SqlConnection con = new SqlConnection(_connectionString))
//            {
//                con.Open();
//                SqlCommand cmd = new SqlCommand("DELETE FROM tblStaging WHERE id IS NULL OR id IN (SELECT RespondentID FROM tblRespondent)", con);
//                cmd.ExecuteNonQuery();
//            }
//        }

//        private void MoveDataToRespondent()
//        {
//            using (CallCentreTrackerEntities1 _db = new CallCentreTrackerEntities1())
//            {
//                var stagingRecords = _db.tblStagings.ToList();

//                foreach (var sourceRecord in stagingRecords)
//                {
//                    if (!_db.tblRespondents.Any(r => r.Resp_Surname == sourceRecord.id.ToString()))
//                    {
//                        tblRespondent newRecord = new tblRespondent
//                        {
//                            Resp_Surname = sourceRecord.id.ToString(),
//                            Resp_FirstName = sourceRecord.CellDescription,
//                            Resp_Oth = sourceRecord.technicalid,
//                            PhoneNumber = sourceRecord.cellid.ToString(),
//                            Q607_C_1 = sourceRecord.celltarget.ToString(),
//                            Q608a_q608_C_1 = sourceRecord.cellpreference,
//                            DateAdded = DateTime.Now,
//                            StatusID = 1,
//                            CheckIn1 = false,
//                            CheckIn2 = false,
//                            CheckIn3 = false,
//                            CheckIn4 = false,
//                            CheckIn5 = false,
//                            PhysicalAttempt1 = false,
//                            PhysicalAttempt2 = false,
//                            PhysicalAttempt3 = false
//                        };

//                        _db.tblRespondents.Add(newRecord);
//                    }
//                }
//                _db.SaveChanges();
//            }
//        }

//        private DataTable ReadExcelToDataTable(string filePath)
//        {
//            DataTable dt = new DataTable();

//            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
//            {
//                WorkbookPart workbookPart = doc.WorkbookPart;
//                Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
//                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
//                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
//                IEnumerable<Row> rows = sheetData.Elements<Row>();

//                foreach (Cell cell in rows.ElementAt(0))
//                {
//                    dt.Columns.Add(GetCellValue(doc, cell));
//                }

//                foreach (Row row in rows.Skip(1))
//                {
//                    DataRow tempRow = dt.NewRow();
//                    int columnIndex = 0;

//                    foreach (Cell cell in row.Elements<Cell>())
//                    {
//                        int cellColumnIndex = GetColumnIndex(cell.CellReference);
//                        while (columnIndex < cellColumnIndex)
//                        {
//                            tempRow[columnIndex] = "";
//                            columnIndex++;
//                        }
//                        tempRow[columnIndex] = GetCellValue(doc, cell);
//                        columnIndex++;
//                    }

//                    dt.Rows.Add(tempRow);
//                }
//            }
//            return dt;
//        }

//        private string GetCellValue(SpreadsheetDocument document, Cell cell)
//        {
//            if (cell.CellValue == null) return "";
//            string value = cell.CellValue.InnerText;
//            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
//            {
//                return document.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
//            }
//            return value;
//        }

//        private int GetColumnIndex(string cellReference)
//        {
//            int index = 0;
//            foreach (char ch in cellReference.Where(Char.IsLetter))
//            {
//                index = (index * 26) + (ch - 'A' + 1);
//            }
//            return index - 1;
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using CallCentreFollowUps.Models;

namespace CallCentreFollowUps.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CallCentreDB"].ConnectionString;

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportExcel()
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    TempData["Error"] = "No file uploaded.";
                    return RedirectToAction("Index");
                }

                HttpPostedFileBase file = Request.Files[0];
                string fileName = Path.GetFileName(file.FileName);
                string path = Server.MapPath("~/FilesUploaded/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, fileName);
                file.SaveAs(filePath);

                DataTable dt = ReadExcelToDataTable(filePath);
                int userCountryID = GetUserCountryID();

                ClearStagingTable();
                BulkInsertIntoStaging(dt, userCountryID);
                RemoveDuplicatesInStaging();
                RemoveExistingRespondents();
                MoveDataToRespondent();

                TempData["Success"] = "File uploaded and processed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing file: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private int GetUserCountryID()
        {
            using (var db = new CallCentreTrackerEntities1())
            {
                string loggedInUser = User.Identity.Name;
                return db.aspnet_Users
                    .Where(u => u.UserName == loggedInUser)
                    .Select(u => (int?)u.CountryID) // Cast to nullable int
                    .FirstOrDefault() ?? 0; // Default to 0 if null
            }
        }

        private void BulkInsertIntoStaging(DataTable dt, int userCountryID)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
                {
                    bulkCopy.DestinationTableName = "tblStaging";

                    bulkCopy.ColumnMappings.Add("id", "id");
                    bulkCopy.ColumnMappings.Add("CellDescription", "CellDescription");
                    bulkCopy.ColumnMappings.Add("technicalid", "technicalid");
                    bulkCopy.ColumnMappings.Add("cellid", "cellid");
                    bulkCopy.ColumnMappings.Add("celltarget", "celltarget");
                    bulkCopy.ColumnMappings.Add("cellpreference", "cellpreference");

                    con.Open();
                    bulkCopy.WriteToServer(dt);
                }
            }
        }

        private void ClearStagingTable()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM tblStaging", con);
                cmd.ExecuteNonQuery();
            }
        }

        private void RemoveDuplicatesInStaging()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    WITH CTE_Duplicates AS (
                        SELECT *, ROW_NUMBER() OVER (PARTITION BY id ORDER BY Id) AS row_num 
                        FROM tblStaging
                    )
                    DELETE FROM CTE_Duplicates WHERE row_num > 1", con);
                cmd.ExecuteNonQuery();
            }
        }

        private void RemoveExistingRespondents()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM tblStaging WHERE id IS NULL OR id IN (SELECT RespondentID FROM tblRespondent)", con);
                cmd.ExecuteNonQuery();
            }
        }

        private void MoveDataToRespondent()
        {
            using (CallCentreTrackerEntities1 _db = new CallCentreTrackerEntities1())
            {
                var stagingRecords = _db.tblStagings.ToList();

                foreach (var sourceRecord in stagingRecords)
                {
                    if (!_db.tblRespondents.Any(r => r.Resp_Surname == sourceRecord.id.ToString()))
                    {
                        tblRespondent newRecord = new tblRespondent
                        {
                            Resp_Surname = sourceRecord.id.ToString(),
                            Resp_FirstName = sourceRecord.CellDescription,
                            Resp_Oth = sourceRecord.technicalid,
                            PhoneNumber = sourceRecord.cellid.ToString(),
                            Q607_C_1 = sourceRecord.celltarget.ToString(),
                            Q608a_q608_C_1 = sourceRecord.cellpreference,
                            DateAdded = DateTime.Now,
                            StatusID = 1,
                            CheckIn1 = false,
                            CheckIn2 = false,
                            CheckIn3 = false,
                            CheckIn4 = false,
                            CheckIn5 = false,
                            PhysicalAttempt1 = false,
                            PhysicalAttempt2 = false,
                            PhysicalAttempt3 = false,
                            CountryID=GetUserCountryID()
                        };

                        _db.tblRespondents.Add(newRecord);
                    }
                }
                _db.SaveChanges();
            }
        }

        private DataTable ReadExcelToDataTable(string filePath)
        {
            DataTable dt = new DataTable();

            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = doc.WorkbookPart;
                Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                IEnumerable<Row> rows = sheetData.Elements<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(doc, cell));
                }

                foreach (Row row in rows.Skip(1))
                {
                    DataRow tempRow = dt.NewRow();
                    int columnIndex = 0;

                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        int cellColumnIndex = GetColumnIndex(cell.CellReference);
                        while (columnIndex < cellColumnIndex)
                        {
                            tempRow[columnIndex] = "";
                            columnIndex++;
                        }
                        tempRow[columnIndex] = GetCellValue(doc, cell);
                        columnIndex++;
                    }

                    dt.Rows.Add(tempRow);
                }
            }
            return dt;
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell.CellValue == null) return "";
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                return document.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            return value;
        }

        private int GetColumnIndex(string cellReference)
        {
            int index = 0;
            foreach (char ch in cellReference.Where(Char.IsLetter))
            {
                index = (index * 26) + (ch - 'A' + 1);
            }
            return index - 1;
        }
    }
}


