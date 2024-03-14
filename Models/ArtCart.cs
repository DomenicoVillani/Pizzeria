using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pizzeria.Models
{
    public class ArtCart
    {
        public Articoli Articolo { get; set; }
        public int Quantita { get; set; }
        public int User_Id { get; set;}
    }
}