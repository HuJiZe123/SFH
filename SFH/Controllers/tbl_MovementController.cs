﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SFH.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Globalization;

namespace SFH.Controllers
{
    public class tbl_MovementController : Controller
    {
        private DB_FinancialHelperEntities1 db = new DB_FinancialHelperEntities1();
        public IRestResponse GetResponse()
        {
            var client = new RestClient("https://currencyscoop.p.rapidapi.com/latest");         // Direccion del servicio
            var request = new RestRequest(Method.GET);                                          // Request del metodo get
            request.AddHeader("x-rapidapi-host", "currencyscoop.p.rapidapi.com");               // Envio de Credenciales
            request.AddHeader("x-rapidapi-key", "d4f47ae288msh5a4ceb34a8c4c91p179bb8jsn4dc74137ed98");
            IRestResponse response = client.Execute(request);                                   // Respuesta en formato Json
            return response;
        }

        [HttpPost]
        public JsonResult GetValuesFromCurrency(string nameCurrency)
        {

            string rateValue = "";
            var response = GetResponse();

            try
            {
                RootObject rootObj;                                                             // Clase para convertir el json a claes
                var json = response.Content;
                rootObj = JsonConvert.DeserializeObject<RootObject>(json);                      // Deserializacion del json
                Rates rates = rootObj.response.rates;
                foreach (var prop in rates.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (prop.Name.ToString() == nameCurrency)
                    {
                        rateValue = prop.GetValue(rates, null).ToString();
                    }
                }
            }
            catch (Exception ex) { }
            return Json(rateValue, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CalculateTotal(string currencyValue, string amount)
        {
            var totalValue = "";
            try
            {
                totalValue = (Convert.ToDouble(amount) / Convert.ToDouble(currencyValue)).ToString();
            }
            catch (Exception ex) { }
            return Json(totalValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CalculateTotal2(string quantity, string amount)
        {
            var totalValue = "";
            try
            {
                totalValue = (Convert.ToDouble(amount) * Convert.ToDouble(quantity)).ToString();
            }
            catch (Exception ex) { }
            return Json(totalValue, JsonRequestBehavior.AllowGet);
        }

        public List<string> GetAllCurrenciesNames()
        {
            var response = GetResponse();
            List<string> currenciesList = new List<string>();

            try
            {
                RootObject rootObj;
                var json = response.Content;
                rootObj = JsonConvert.DeserializeObject<RootObject>(json);
                Rates rates = rootObj.response.rates;
                foreach (var prop in rates.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    currenciesList.Add(prop.Name.ToString());
                }
            }
            catch (Exception ex) { }
            return currenciesList;
        }

        public float GetTotalInAList(List<tbl_Movement> movements)
        {
            float activeAmount = 0;                                             // variables que contendran los valores a examinar
            float pasiveAmount = 0;
            float totalAmount = 0;
            foreach (var amount in movements)                             // For que nos permite ir adicionanado los ingresos como egresos
            {
                if (amount.type == 1) activeAmount += Convert.ToSingle(amount.Sutotal);           // si es de tpo 1 es ingreso, 0 egreso, asi se asignan a las diferentes variables
                else pasiveAmount += Convert.ToSingle(amount.Sutotal);
            }
            totalAmount = activeAmount - pasiveAmount;                          // Variable que guarda el ingreso o egreso total
            return totalAmount;
        }


        /*********************************************************************************************************/

        // GET: tbl_Movement
        public ActionResult IndexYears()
        {
            var movements = db.tbl_Movement.OrderByDescending(mv => mv.ExecutedDate).ToList();                               // extraer todos los datos 
            List<tbl_Movement> movementsYears = new List<tbl_Movement>();           // Lista que contendra los valores a mostrar
            List<int> allYears = new List<int>();                                   // Lista que contendra todos los años
            float totalAmount = 0;
            foreach (var item in movements)                                          // Filtrado de años, los repetidos se descartan
            {
                if (!allYears.Contains(item.ExecutedDate.Year)) allYears.Add(item.ExecutedDate.Year);
            }
            foreach(var item in allYears)
            {
                tbl_Movement movementYearSingle = new tbl_Movement();               // Objeto que servira para el envio de datos
                totalAmount = 0;
                var movementsPerYear = movements.Where(mpy => mpy.ExecutedDate.Year == item).ToList();     // Lista con los datos del año a evaluar
                totalAmount = GetTotalInAList(movementsPerYear);                          // Variable que guarda el ingreso o egreso total
                movementYearSingle.quantity = item;                                 // asignacion del año a la variable quality, de tipo entero
                if (totalAmount > 0) movementYearSingle.type = 1;                   // filtrado para ver el tipo de monto, si es ingreso o egreso
                else movementYearSingle.type = 0;   
                movementYearSingle.Sutotal = Math.Abs(totalAmount);  // Eliminacion del signo negativo y asignacion al objeto para el envio
                movementsYears.Add(movementYearSingle);                             /// Adicion del objeto a la lista
            }
            totalAmount = GetTotalInAList(movements);                          // Variable que guarda el ingreso o egreso total
            if (totalAmount > 0) ViewBag.ActualType = 1;
            else ViewBag.ActualType = 0;
            ViewBag.ActualAmount = Math.Abs(totalAmount);
            return View(movementsYears.ToList());                                   // Envio de la lista a la vista
        }

        // GET: tbl_Movement
        public ActionResult IndexMonth(int year)
        {
            var movements = db.tbl_Movement.Where(mv => mv.ExecutedDate.Year == year).OrderByDescending(mv => mv.ExecutedDate).ToList();                               // extraer todos los datos 
            List<tbl_Movement> movementsMonth = new List<tbl_Movement>();           // Lista que contendra los valores a mostrar
            List<int> allMonth = new List<int>();                                   // Lista que contendra todos los años
            float totalAmount = 0;
            foreach (var item in movements)                                          // Filtrado de años, los repetidos se descartan
            {
                if (!allMonth.Contains(item.ExecutedDate.Month)) allMonth.Add(item.ExecutedDate.Month);
            }
            foreach (var item in allMonth)
            {
                tbl_Movement movementMonthSingle = new tbl_Movement();               // Objeto que servira para el envio de datos
                var movementsPerMonth = movements.Where(mpy => mpy.ExecutedDate.Month == item).ToList();     // Lista con los datos del año a evaluar
                totalAmount = GetTotalInAList(movementsPerMonth);                          // Variable que guarda el ingreso o egreso total
                movementMonthSingle.Description = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item);                                 // asignacion del año a la variable quality, de tipo entero
                if (totalAmount > 0) movementMonthSingle.type = 1;                   // filtrado para ver el tipo de monto, si es ingreso o egreso
                else movementMonthSingle.type = 0;
                movementMonthSingle.Sutotal = Math.Abs(totalAmount);  // Eliminacion del signo negativo y asignacion al objeto para el envio
                movementMonthSingle.quantity = year;  // Eliminacion del signo negativo y asignacion al objeto para el envio
                movementsMonth.Add(movementMonthSingle);                             /// Adicion del objeto a la lista
            }
            totalAmount = GetTotalInAList(movements);                          // Variable que guarda el ingreso o egreso total
            if (totalAmount > 0) ViewBag.ActualType = 1;
            else ViewBag.ActualType = 0;
            ViewBag.ActualAmount = Math.Abs(totalAmount);
            ViewBag.ActualYear = year;
            return View(movementsMonth.ToList());                                   // Envio de la lista a la vista
        }

        // GET: tbl_Movement
        public ActionResult Index(int year, string month, int type, float amount)
        {
            AssignValues(year, month, type, amount);
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            int monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;
            var movements = db.tbl_Movement.Where(mv => mv.ExecutedDate.Year == year && mv.ExecutedDate.Month == monthNumber).OrderByDescending(mv => mv.ExecutedDate).ToList();                               // extraer todos los datos 
            return View(movements.ToList());                                   // Envio de la lista a la vista
        }

        public ActionResult RegisteredValues()
        {
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            var registeredValue = db.tbl_ConstantValues.Where(cv => cv.status == 1).ToList();
            return View(registeredValue);
        }

        // GET: tbl_Movement/Details/5
        public ActionResult Details(int? id)
        {
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Movement tbl_Movement = db.tbl_Movement.Find(id);
            if (tbl_Movement == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Movement);
        }

        // GET: tbl_Movement/Create
        public ActionResult Create()
        {
            ViewData["Currency"] = GetAllCurrenciesNames().Select(r =>
              new SelectListItem()
              {
                  Text = r.ToString()
              });
            ViewBag.txtBoxValueCurrency = "1";
            ViewBag.txtBoxAmount = "0";
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            return View();
        }

        public void AssignValues(int year, string month, int type, float amount)
        {
            /**********************************************/
            YearInMonth = year;
            ActualMonth = month;
            ActualAmount = amount;
            ActualType = type;
            /**********************************************/
        }

        // GET: tbl_Movement/Details/5
        public ActionResult AddInformationFlow(int? identificador)
        {
            if (identificador == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int monthNumber = DateTime.ParseExact(ActualMonth, "MMMM", CultureInfo.CurrentCulture).Month; 
            string dateS = YearInMonth + "-" + monthNumber + "-" + "1";
            DateTime date = DateTime.Parse(dateS);
            tbl_ConstantValues cv = db.tbl_ConstantValues.Find(identificador);
            tbl_Movement tbl_Movement = new tbl_Movement();
            tbl_Movement.Description = cv.Description;
            tbl_Movement.Cost = cv.Value;
            tbl_Movement.quantity = 1;
            tbl_Movement.Sutotal = cv.Value * tbl_Movement.quantity;
            tbl_Movement.ExecutedDate = date;
            tbl_Movement.ExpectedDate = date;
            tbl_Movement.State = 0;
            tbl_Movement.type = 1;
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            if (tbl_Movement == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Movement);
        }

        /*-------------------------------------------------*/

        public static int YearInMonth { get; set; }
        public static string ActualMonth { get; set; }
        public static float ActualAmount { get; set; }
        public static int ActualType { get; set; }

        /*-------------------------------------------------*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInformationFlow([Bind(Include = "id,Description,Cost,quantity,Sutotal,ExpectedDate,ExecutedDate,State,type")] tbl_Movement tbl_Movement)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Movement.Add(tbl_Movement);
                db.SaveChanges();
                return RedirectToAction("RegisteredValues", new { year = YearInMonth, month = ActualMonth, type = ActualType, amount = ActualAmount });
            }

            return View(tbl_Movement);
        }

        // POST: tbl_Movement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Description,Cost,quantity,Sutotal,ExpectedDate,ExecutedDate,State,type")] tbl_Movement tbl_Movement)
        {
            if (ModelState.IsValid)
            {
                db.tbl_Movement.Add(tbl_Movement);
                db.SaveChanges();
                return RedirectToAction("Index", new { year = YearInMonth, month = ActualMonth, type = ActualType, amount = ActualAmount });
            }

            return View(tbl_Movement);
        }

        // GET: tbl_Movement/Edit/5
        public ActionResult Edit(int? id)
        {
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Movement tbl_Movement = db.tbl_Movement.Find(id);
            if (tbl_Movement == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Movement);
        }

        // POST: tbl_Movement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Description,Cost,quantity,Sutotal,ExpectedDate,ExecutedDate,State,type")] tbl_Movement tbl_Movement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_Movement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { year = YearInMonth, month = ActualMonth, type = ActualType, amount = ActualAmount });
            }
            return View(tbl_Movement);
        }

        // GET: tbl_Movement/Delete/5
        public ActionResult Delete(int? id)
        {
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_Movement tbl_Movement = db.tbl_Movement.Find(id);
            if (tbl_Movement == null)
            {
                return HttpNotFound();
            }
            return View(tbl_Movement);
        }

        // POST: tbl_Movement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            /*****************************************************************/
            ViewBag.ActualType = ActualType;
            ViewBag.ActualAmount = ActualAmount;
            ViewBag.ActualMonth = ActualMonth;
            ViewBag.YearInMonth = YearInMonth;
            /*****************************************************************/
            tbl_Movement tbl_Movement = db.tbl_Movement.Find(id);
            db.tbl_Movement.Remove(tbl_Movement);
            db.SaveChanges();
            return RedirectToAction("Index", new { year = YearInMonth, month = ActualMonth, type = ActualType, amount = ActualAmount });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        /**********************************************************************************************/

        public class Meta
        {
            public int code { get; set; }
            public string disclaimer { get; set; }
        }

        public class Rates
        {
            public int USD { get; set; }
            public double EUR { get; set; }
            public double GBP { get; set; }
            public double INR { get; set; }
            public double AUD { get; set; }
            public double CAD { get; set; }
            public double SGD { get; set; }
            public double CHF { get; set; }
            public double MYR { get; set; }
            public double JPY { get; set; }
            public double CNY { get; set; }
            public double NZD { get; set; }
            public double THB { get; set; }
            public double HUF { get; set; }
            public double AED { get; set; }
            public double HKD { get; set; }
            public double MXN { get; set; }
            public double ZAR { get; set; }
            public double PHP { get; set; }
            public double SEK { get; set; }
            public double IDR { get; set; }
            public double SAR { get; set; }
            public double BRL { get; set; }
            public double TRY { get; set; }
            public double KES { get; set; }
            public double KRW { get; set; }
            public double EGP { get; set; }
            public double IQD { get; set; }
            public double NOK { get; set; }
            public double KWD { get; set; }
            public double RUB { get; set; }
            public double DKK { get; set; }
            public double PKR { get; set; }
            public double ILS { get; set; }
            public double PLN { get; set; }
            public double QAR { get; set; }
            public double XAU { get; set; }
            public double OMR { get; set; }
            public double COP { get; set; }
            public double CLP { get; set; }
            public double TWD { get; set; }
            public double ARS { get; set; }
            public double CZK { get; set; }
            public double VND { get; set; }
            public double MAD { get; set; }
            public double JOD { get; set; }
            public double BHD { get; set; }
            public double XOF { get; set; }
            public double LKR { get; set; }
            public double UAH { get; set; }
            public double NGN { get; set; }
            public double TND { get; set; }
            public double UGX { get; set; }
            public double RON { get; set; }
            public double BDT { get; set; }
            public double PEN { get; set; }
            public double GEL { get; set; }
            public double XAF { get; set; }
            public double FJD { get; set; }
            public double VEF { get; set; }
            public double VES { get; set; }
            public double BYN { get; set; }
            public double HRK { get; set; }
            public double UZS { get; set; }
            public double BGN { get; set; }
            public double DZD { get; set; }
            public double IRR { get; set; }
            public double DOP { get; set; }
            public double ISK { get; set; }
            public double XAG { get; set; }
            public double CRC { get; set; }
            public double SYP { get; set; }
            public double LYD { get; set; }
            public double JMD { get; set; }
            public double MUR { get; set; }
            public double GHS { get; set; }
            public double AOA { get; set; }
            public double UYU { get; set; }
            public double AFN { get; set; }
            public double LBP { get; set; }
            public double XPF { get; set; }
            public double TTD { get; set; }
            public double TZS { get; set; }
            public double ALL { get; set; }
            public double XCD { get; set; }
            public double GTQ { get; set; }
            public double NPR { get; set; }
            public double BOB { get; set; }
            public double ZWD { get; set; }
            public int BBD { get; set; }
            public int CUC { get; set; }
            public double LAK { get; set; }
            public double BND { get; set; }
            public double BWP { get; set; }
            public double HNL { get; set; }
            public double PYG { get; set; }
            public double ETB { get; set; }
            public double NAD { get; set; }
            public double PGK { get; set; }
            public double SDG { get; set; }
            public double MOP { get; set; }
            public double NIO { get; set; }
            public int BMD { get; set; }
            public double KZT { get; set; }
            public int PAB { get; set; }
            public double BAM { get; set; }
            public double GYD { get; set; }
            public double YER { get; set; }
            public double MGA { get; set; }
            public double KYD { get; set; }
            public double MZN { get; set; }
            public double RSD { get; set; }
            public double SCR { get; set; }
            public double AMD { get; set; }
            public double SBD { get; set; }
            public double AZN { get; set; }
            public double SLL { get; set; }
            public double TOP { get; set; }
            public double BZD { get; set; }
            public double MWK { get; set; }
            public double GMD { get; set; }
            public double BIF { get; set; }
            public double SOS { get; set; }
            public double HTG { get; set; }
            public double GNF { get; set; }
            public double MVR { get; set; }
            public double MNT { get; set; }
            public double CDF { get; set; }
            public double STN { get; set; }
            public double TJS { get; set; }
            public double KPW { get; set; }
            public double MMK { get; set; }
            public double LSL { get; set; }
            public double LRD { get; set; }
            public double KGS { get; set; }
            public double GIP { get; set; }
            public double XPT { get; set; }
            public double MDL { get; set; }
            public double CUP { get; set; }
            public double KHR { get; set; }
            public double MKD { get; set; }
            public double VUV { get; set; }
            public double MRU { get; set; }
            public double ANG { get; set; }
            public double SZL { get; set; }
            public double CVE { get; set; }
            public double SRD { get; set; }
            public double XPD { get; set; }
            public double SVC { get; set; }
            public int BSD { get; set; }
            public double XDR { get; set; }
            public double RWF { get; set; }
            public double AWG { get; set; }
            public double DJF { get; set; }
            public double BTN { get; set; }
            public double KMF { get; set; }
            public double WST { get; set; }
            public double SPL { get; set; }
            public double ERN { get; set; }
            public double FKP { get; set; }
            public double SHP { get; set; }
            public double JEP { get; set; }
            public double TMT { get; set; }
            public double TVD { get; set; }
            public double IMP { get; set; }
            public double GGP { get; set; }
            public double ZMW { get; set; }
        }

        public class Response
        {
            public DateTime date { get; set; }
            public string @base { get; set; }
            public Rates rates { get; set; }
        }

        public class RootObject
        {
            public Meta meta { get; set; }
            public Response response { get; set; }
        }

        /**********************************************************************************************/
    }
}

