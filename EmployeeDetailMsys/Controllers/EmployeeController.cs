﻿using EmployeeDetailMsys.DB;
using EmployeeDetailMsys.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using OfficeOpenXml;
using Newtonsoft.Json;
using System.Net;
using PagedList;
using Rotativa;

namespace EmployeeDetailMsys.Controllers
{
    public class EmployeeController : Controller
    {
        DBConn conn = new DBConn();
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }


       

        public ActionResult ImportData()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ImportData(HttpPostedFileBase postedfile)
        {
            string path = Server.MapPath("~/Excelfiles/");
            bool isValidationSuccess = true;
            string validationMessage = string.Empty;


            if (postedfile != null)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Path.GetFileName(postedfile.FileName);
                string serverPath = Path.Combine(path, fileName);
                postedfile.SaveAs(serverPath);

                List<ImportM> datas = new List<ImportM>();
                FileInfo fileInfo = new FileInfo(serverPath);

                ExcelPackage package = new ExcelPackage(fileInfo);
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

                // get number of rows and columns in the sheet
                int rows = worksheet.Dimension.Rows; // 20
                int columns = worksheet.Dimension.Columns; // 7

                // loop through the worksheet rows and columns
                for (int i = 1; i <= rows; i++)
                {
                    datas.Add(new ImportM
                    {
                        Index = i,
                        FullName = worksheet.Cells[i, 1]?.Value?.ToString(),
                        DateOfBirth = worksheet.Cells[i, 2].Value == null ? null : Convert.ToDateTime(worksheet.Cells[i, 2].Value).ToString("yyyy/MM/dd"),
                        Gender = worksheet.Cells[i, 3]?.Value?.ToString(),
                        Salary = worksheet.Cells[i, 4]?.Value == null ? 0.00M : Convert.ToDecimal(worksheet.Cells[i, 4]?.Value),
                        Designation = worksheet.Cells[i, 5]?.Value?.ToString(),
                        importdate = DateTime.Now.ToString("yyyy/MM/dd")
                    });
                }

                int[] arr = Array.Empty<int>();
                arr = datas.Where(x => string.IsNullOrEmpty(x.FullName)
                                        && string.IsNullOrEmpty(x.DateOfBirth)
                                        && string.IsNullOrEmpty(x.Gender)
                                        && string.IsNullOrEmpty(x.Designation))
                    .Select(x => x.Index).ToArray();
               
                if (arr.Any())
                {
                    validationMessage = $"X rows [{String.Join(",", arr)}] were skipped since they did not have data";
                    isValidationSuccess = false;
                }

                if (!arr.Any() && datas.Any(x => string.IsNullOrEmpty(x.FullName)))
                {
                    validationMessage = "Invalid excel. Full name can not be empty.";
                    isValidationSuccess = false;
                }
                if (!arr.Any() && datas.Any(x => string.IsNullOrEmpty(x.DateOfBirth)))
                {
                    validationMessage = "Invalid excel. Date Of Birth can not be empty.";
                    isValidationSuccess = false;
                }
                if (!arr.Any() && datas.Any(x => string.IsNullOrEmpty(x.Gender)))
                {
                    validationMessage = "Invalid excel. Gender can not be empty.";
                    isValidationSuccess = false;
                }
                if (!arr.Any() && datas.Any(x => x.Salary < 0))
                {
                    validationMessage = "Invalid excel. Salary can not be less than zero.";
                    isValidationSuccess = false;
                }
                if (!arr.Any() && datas.Any(x => string.IsNullOrEmpty(x.Designation)))
                {
                    validationMessage = "Invalid excel. Designation can not be empty.";
                    isValidationSuccess = false;
                }

                if (isValidationSuccess)
                {
                    string jsondata = JsonConvert.SerializeObject(datas);
                    conn.SaveImportData(jsondata, out string message);
                }

            }

            ViewBag.Validation = new ResponseModel
            {
                IsSuccess = isValidationSuccess,
                Message = validationMessage
            };
            return View();
            //return RedirectToAction("FetchImportData");
        }

        [HttpGet]
        public ActionResult FetchImportData()
        {

            List<ImportM> importList = conn.GetImportData();
            

            return View(importList);

        }

            [HttpPost]
        public ActionResult Index(EmployeeM emp, HttpPostedFileBase file)
        {

            if (file != null && file.ContentLength > 0)
            {
                string filename = Path.GetFileName(file.FileName);
                string imgpath = Path.Combine(Server.MapPath("~/UserImages/"), filename);
                file.SaveAs(imgpath);
            }
            string message;
            conn.SaveData(emp, file, out message);
            ViewBag.Message = "Employee Record is Added Successfully";
            //return View("Index");
            return RedirectToAction("EmployeeRecord");
        }

       

        [HttpGet]
        public ActionResult EmployeeRecord()
        {
            List<EmployeeM> resultList = conn.GetEmployeeData();

            return View(resultList);
        }

        public ActionResult PrintViewToPdf()
        {
            var report = new ActionAsPdf("EmployeeRecord");
            return report;
        }

        public ActionResult Edit()
        {

            return View();

        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            EmployeeM EditList = conn.EditData(id);
            return View(EditList);

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeM emp, HttpPostedFileBase file)
        {


            if (file != null && file.ContentLength > 0)
            {
                string filename = Path.GetFileName(file.FileName);
                string imgpath = Path.Combine(Server.MapPath("~/UserImages/"), filename);
                file.SaveAs(imgpath);
            }

            string message;
            conn.UpdateData(emp, file, out message);
            //return Json(new JsonResult { Data = message });
            return RedirectToAction("EmployeeRecord");

        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeM EditList = conn.EditData(id);
            return View(EditList);

        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            string message;
            conn.DeleteEmpData(id, out message);


            //return View();
            ViewBag.Message = "Employee Record Delete Successfully";
            return RedirectToAction("EmployeeRecord");
            
        }
    }
}