using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FastReport.Data;
using FastReport.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System;
using CoreProject.Data;
using Microsoft.AspNetCore.Http;
using static CoreProject.Controllers.MechanicsServices;
using System.Linq;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
namespace CoreProject.Controllers
{
    public class MechanicsServices : Controller
    {
        private readonly IConfiguration _configuration;
        private ApplicationDbContext db;
        private IHostingEnvironment HostEnvironment;//file upload in specific folder
        public MechanicsServices(SignInManager<IdentityUser> signInManager, ApplicationDbContext _db, IHostingEnvironment _HostEnvironment, IConfiguration configuration)
        {
            _signInManager = signInManager;
            db = _db;
            HostEnvironment = _HostEnvironment;
            _configuration = configuration;
        }
        private readonly SignInManager<IdentityUser> _signInManager;
        

        public IActionResult Report()
        {
            if (!_signInManager.IsSignedIn(User)) //verify if it's logged
            {
                return LocalRedirect("~/");
            }          
            var webReport = new WebReport();
            var mssqlDataConnection = new MsSqlDataConnection();
            mssqlDataConnection.ConnectionString = _configuration.GetConnectionString("Core2Context");
            webReport.Report.Dictionary.Connections.Add(mssqlDataConnection);
            webReport.Report.Load(Path.Combine(HostEnvironment.ContentRootPath, "reports", "mechanic.frx"));
            var mechanic = GetTable<mechanic>(db.mechanic, "mechanic");
            webReport.Report.RegisterData(mechanic, "mechanic");
            return View(webReport);
        }


        static DataTable GetTable<TEntity>(IEnumerable<TEntity> table, string name) where TEntity : class
        {
            var offset = 78;
            DataTable result = new DataTable(name);
            PropertyInfo[] infos = typeof(TEntity).GetProperties();
            foreach (PropertyInfo info in infos)
            {
                if (info.PropertyType.IsGenericType
                && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    result.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType)));
                }
                else
                {
                    result.Columns.Add(new DataColumn(info.Name, info.PropertyType));
                }
            }
            foreach (var el in table)
            {
                DataRow row = result.NewRow();
                foreach (PropertyInfo info in infos)
                {
                    if (info.PropertyType.IsGenericType &&
                    info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        object t = info.GetValue(el);
                        if (t == null)
                        {
                            t = Activator.CreateInstance(Nullable.GetUnderlyingType(info.PropertyType));
                        }

                        row[info.Name] = t;
                    }
                    else
                    {
                        if (info.PropertyType == typeof(byte[]))
                        {
                            //Fix for Image issue.
                            var imageData = (byte[])info.GetValue(el);
                            var bytes = new byte[imageData.Length - offset];
                            Array.Copy(imageData, offset, bytes, 0, bytes.Length);
                            row[info.Name] = bytes;
                        }
                        else
                        {
                            row[info.Name] = info.GetValue(el);
                        }
                    }
                }
                result.Rows.Add(row);
            }

            return result;
        }

        //All Codes of previous masterdetails crud
        [Route("myroute")]
        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User)) //verify if it's logged
            {
                ViewBag.code = GenerateCode();
                ViewBag.code2 = GenerateCode2();
                return View();
            }
            else
                return LocalRedirect("~/");
           
        }
        public string GenerateCode()
        {
            string a1 = "";
            string b1 = "";

            try
            {
                var a = (from det in db.mechanic select det.MechanicId.Substring(3)).Max();
                if (a == null)
                    a = "0";
                int b = int.Parse(a.ToString()) + 1;
                if (b < 10)
                {
                    b1 = "000" + b.ToString();
                }
                else if (b < 100)
                {
                    b1 = "00" + b.ToString();
                }
                else if (b < 1000)
                {
                    b1 = "0" + b.ToString();
                }
                else
                {
                    b1 = b.ToString();
                }
                a1 = "AC-" + b1.ToString();
            }
            catch (Exception ex)
            {
                a1 = "AC-0001";
            }
            return a1;
        }

        public string GenerateCode2()
        {
            string a1 = "";
            string b1 = "";

            try
            {
                var a = (from det in db.service select det.ServiceId.Substring(2)).Max();
                if (a == null)
                    a = "0";
                int b = int.Parse(a.ToString()) + 1;
                if (b < 10)
                {
                    b1 = "000" + b.ToString();
                }
                else if (b < 100)
                {
                    b1 = "00" + b.ToString();
                }
                else if (b < 1000)
                {
                    b1 = "0" + b.ToString();
                }
                else
                {
                    b1 = b.ToString();
                }
                a1 = "S-" + b1.ToString();
            }
            catch (Exception ex)
            {
                a1 = "S-0001";
            }
            return a1;
        }
        public JsonResult GetMechanic(string id)
        {
            var a = (from d in db.mechanic where d.MechanicId == id select new { d.Name, d.Image, d.Active, d.Address });
            return Json(a);
        }
        public JsonResult GetService(string id)
        {
            var a = (from d in db.service where d.MechanicId == id select new { d.ServiceId, d.Date, d.Description, d.slno });
            return Json(a);
        }

        public JsonResult InsertDept(mechanic stu_Guard)
        {
            mechanic a = new mechanic() { MechanicId = stu_Guard.MechanicId, Active = stu_Guard.Active, Address = stu_Guard.Address, Image = stu_Guard.Image, Name = stu_Guard.Name };
            db.mechanic.Add(a);
            db.SaveChanges();
            return Json(a);
        }
        public JsonResult InsertItems(services stu_Guard)
        {
            services a1 = new services() { ServiceId = stu_Guard.ServiceId, Date = stu_Guard.Date, Description = stu_Guard.Description, MechanicId = stu_Guard.MechanicId, slno = stu_Guard.slno };
            db.service.Add(a1);
            db.SaveChanges();
            return Json(a1);
        }
       
        public record Mechanic2
        {
            //primary/master table
            public string MechanicId { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public bool Active { get; set; }
            public string Image { get; set; }
            //child/details table
            public services[] servicelink { get; set; }
        }
        [HttpPost]
        public JsonResult AddMasterDetails([FromBody] Mechanic2 stu_Guard)
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {
                mechanic m = new mechanic() { MechanicId = stu_Guard.MechanicId, Active = stu_Guard.Active, Address = stu_Guard.Address, Image = stu_Guard.Image, Name = stu_Guard.Name };
                db.mechanic.Add(m);

                foreach (var c in stu_Guard.servicelink)
                {
                    services d = new services() { ServiceId = c.ServiceId, Date = c.Date, Description = c.Description, MechanicId = stu_Guard.MechanicId, slno = c.slno };
                    db.service.Add(d);
                }
                db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json("Error");
                // Other steps for handling failures
            }
            return Json(stu_Guard);
        }


        public JsonResult DeleteAll(string id)
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {
                List<services> st5 = db.service.Where(xx => xx.MechanicId == id).ToList();
                db.service.RemoveRange(st5);

                mechanic st6 = db.mechanic.Find(id);
                if (st6 != null)
                {
                    db.mechanic.Remove(st6);
                }
                db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json("Error");
                // Other steps for handling failures
            }
            return Json("OK");
        }
        [HttpPost]
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    var files = Request.Form.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        string fname;

                        fname = file.FileName;
                        string webRootPath = HostEnvironment.WebRootPath;
                        string fname1 = Path.Combine(webRootPath, "Uploads/" + fname);
                        file.CopyTo(new FileStream(fname1, FileMode.Create));
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        public string Child_Exists(string id)
        {
            var a = (from p in db.service where p.ServiceId == id select new { p.ServiceId, }).FirstOrDefault();
            if (a != null)
                return "1";
            else
                return "0";
        }
    }
}
