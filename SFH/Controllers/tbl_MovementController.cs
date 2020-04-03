using System;
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


        /*********************************************************************************************************/

        // GET: tbl_Movement
        public ActionResult IndexMonth()
        {
            return View(db.tbl_Movement.ToList());
        }

        // GET: tbl_Movement
        public ActionResult Index()
        {
            return View(db.tbl_Movement.ToList());
        }

        // GET: tbl_Movement/Details/5
        public ActionResult Details(int? id)
        {
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
            return View();
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
                return RedirectToAction("Index");
            }

            return View(tbl_Movement);
        }

        // GET: tbl_Movement/Edit/5
        public ActionResult Edit(int? id)
        {
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
                return RedirectToAction("Index");
            }
            return View(tbl_Movement);
        }

        // GET: tbl_Movement/Delete/5
        public ActionResult Delete(int? id)
        {
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
            tbl_Movement tbl_Movement = db.tbl_Movement.Find(id);
            db.tbl_Movement.Remove(tbl_Movement);
            db.SaveChanges();
            return RedirectToAction("Index");
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

