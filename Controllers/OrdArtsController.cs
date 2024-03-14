using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Pizzeria.Models;

namespace Pizzeria.Controllers
{
    public class OrdArtsController : Controller
    {
        private DBContext db = new DBContext();

        // GET: OrdArts
        [Authorize(Roles = "Amministratore")]
        public ActionResult Index()
        {
            var ordArt = db.OrdArt.Include(o => o.Articoli).Include(o => o.Ordini);
            return View(ordArt.ToList());
        }

        // GET: OrdArts/Details/5
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ordineWithArticoli = db.OrdArt
                .Include(o => o.Ordini)
                .Include(o => o.Ordini.Users)
                .Include(o => o.Articoli)
                .Where(o => o.Ordine_ID == id).ToList();

            if (ordineWithArticoli == null)
            {
                return HttpNotFound();
            }

            return View(ordineWithArticoli);

            //var ArtOrderId = db.OrdArt.Where(u => u.Ordine_ID == orderId).ToList();
            //Ordini ordini = db.Ordini.Find(orderId);
            //if (ArtOrderId == null || ordini == null)
            //{
            //    return HttpNotFound();
            //}
            //TempData["ordineDetails"] = ordini;
            //return View(ArtOrderId);
        }

        // GET: OrdArts/Create
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Create()
        {
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome");
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo");
            return View();
        }

        // POST: OrdArts/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Amministratore")]
        public ActionResult Create([Bind(Include = "Articolo_ID,Ordine_ID,Quantita")] OrdArt ordArt)
        {
            var ControlloOrdine = db.OrdArt.Where(o => o.Articolo_ID == ordArt.Articolo_ID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (ControlloOrdine == null)
                {
                    db.OrdArt.Add(ordArt);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ControlloOrdine.Quantita += ordArt.Quantita;
                    db.Entry(ControlloOrdine).State = EntityState.Modified;
                }
            }
            ViewBag.Articolo_ID = new SelectList(db.Articoli, "Articolo_ID", "Nome", ordArt.Articolo_ID);
            ViewBag.Ordine_ID = new SelectList(db.Ordini, "Ordine_ID", "Indirizzo", ordArt.Ordine_ID);
            return View(ordArt);
        }

        // GET: OrdArts/Delete/5
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrdArt ordArt = db.OrdArt.Find(id);
            if (ordArt == null)
            {
                return HttpNotFound();
            }
            return View(ordArt);
        }

        // POST: OrdArts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Amministratore,Cliente")]
        public ActionResult DeleteConfirmed(int id)
        {
            OrdArt ordArt = db.OrdArt.Find(id);
            db.OrdArt.Remove(ordArt);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public ActionResult AddToCart(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var articolo = db.Articoli.FirstOrDefault(o => o.Articolo_ID == id);
            if (articolo == null)
            {
                return HttpNotFound();
            }

            // Inizializza artsCart
            List<ArtCart> artsCart = new List<ArtCart>();

            // Creazione del cookie
            HttpCookie cartCookie;

            // Verifica se il cookie "Carrello" esiste già
            if (Request.Cookies["Carrello"] != null && Request.Cookies["Carrello"]["User"] != null)
            {
                // Se esiste, aggiorna direttamente il valore
                cartCookie = Request.Cookies["Carrello"];
                // Decodifica il valore del cookie e riempie la lista
                var cartJson = HttpUtility.UrlDecode(Request.Cookies["Carrello"]["User"]);
                artsCart = JsonConvert.DeserializeObject<List<ArtCart>>(cartJson);
            }
            else
            {
                // Altrimenti, crea un nuovo cookie solo se non esiste già
                cartCookie = new HttpCookie("Carrello");
                cartCookie.Values["User"] = User.Identity.Name;
            }

            // Aggiungi l'articolo al carrello
            var artCart = new ArtCart()
            {
                Articolo = new Articoli()
                {
                    Articolo_ID = articolo.Articolo_ID,
                    Nome = articolo.Nome,
                    Prezzo = articolo.Prezzo,
                    Img = articolo.Img,
                    Tempo_Cons = articolo.Tempo_Cons,
                    Ingredienti = articolo.Ingredienti,
                },
                Quantita = 1,
                User_Id = Convert.ToInt32(User.Identity.Name),
            };
            artsCart.Add(artCart);

            // Serializza la lista e aggiorna il valore del cookie
            cartCookie["User"] = HttpUtility.UrlEncode(JsonConvert.SerializeObject(artsCart));
            cartCookie.Expires = DateTime.Now.AddDays(1);

            // Aggiunge o aggiorna il cookie nella risposta
            Response.Cookies.Add(cartCookie);

            return RedirectToAction("Index", "Articoli");
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public ActionResult Cart()
        {
            List<ArtCart> userArtCart = new List<ArtCart>();

            // Verifica se il cookie "Carrello" esiste già
            if (Request.Cookies["Carrello"] != null && Request.Cookies["Carrello"]["User"] != null)
            {
                var cartJson = HttpUtility.UrlDecode(Request.Cookies["Carrello"]["User"]);
                var userId = Convert.ToInt32(User.Identity.Name);

                // Decodifica il valore del cookie e riempie la lista
                var artsCart = JsonConvert.DeserializeObject<List<ArtCart>>(cartJson);

                // Filtra solo gli articoli relativi all'utente attuale
                userArtCart = artsCart.Where(a => a.User_Id == userId).ToList();
                ViewBag.UserCart = userArtCart;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrderFromCart(OrdArt ordArt)
        {
            if (ModelState.IsValid)
            {
                ordArt.Ordini.CostoCons = 4;
                ordArt.Ordini.User_ID = Convert.ToInt32(User.Identity.Name);
                db.Ordini.Add(ordArt.Ordini);
                db.SaveChanges();

                int newOrdineID = ordArt.Ordini.Ordine_ID;

                List<ArtCart> userArtCart = new List<ArtCart>();

                // Verifica se il cookie "Carrello" esiste già
                if (Request.Cookies["Carrello"] != null && Request.Cookies["Carrello"]["User"] != null)
                {
                    var cartJson = HttpUtility.UrlDecode(Request.Cookies["Carrello"]["User"]);
                    var userId = Convert.ToInt32(User.Identity.Name);

                    // Decodifica il valore del cookie e riempie la lista
                    var artsCart = JsonConvert.DeserializeObject<List<ArtCart>>(cartJson);

                    // Filtra solo gli articoli relativi all'utente attuale
                    userArtCart = artsCart.Where(a => a.User_Id == userId).ToList();
                    ViewBag.UserCart = userArtCart;
                }

                foreach (ArtCart art in userArtCart)
                {
                    var newOrdArt = new OrdArt();  // Create a new instance of OrdArt for each ArtCart item
                    newOrdArt.Articolo_ID = art.Articolo.Articolo_ID;
                    newOrdArt.Ordine_ID = newOrdineID;
                    newOrdArt.Quantita = Convert.ToInt32(art.Quantita);
                    db.OrdArt.Add(newOrdArt);  // Add the new instance to the database context
                }
                db.SaveChanges();
                return RedirectToAction("Details", "OrdArts", new { id = newOrdineID });
            }
            return RedirectToAction("Cart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
