using Cabinet_Medical.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public DashboardController(CabinetMedicalContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            return role switch
            {
                "Admin" => RedirectToAction("Admin"),
                "Secretaire" => RedirectToAction("Secretaire"),
                "Medecin" => RedirectToAction("Medecin"),
                "Patient" => RedirectToAction("Patient"),
                _ => RedirectToAction("Login", "Account")
            };
        }

        public async Task<IActionResult> Admin()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalMedecins = await _context.Medecins.CountAsync();
            return View();
        }

        public async Task<IActionResult> Secretaire()
        {
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalRDV = await _context.RendezVous.CountAsync();
            ViewBag.RDVAujourdhui = await _context.RendezVous
                .CountAsync(r => r.DateRdv.Date == DateTime.Today && r.Statut == "Planifie");
            ViewBag.FacturesNonPayees = await _context.Factures
                .CountAsync(f => f.Statut == "NonPayee");
            return View();
        }

        public async Task<IActionResult> Medecin()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var medecin = user != null ? await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id) : null;

            if (medecin != null)
            {
                ViewBag.MesRDVAujourdhui = await _context.RendezVous
                    .CountAsync(r => r.MedecinId == medecin.Id && r.DateRdv.Date == DateTime.Today && r.Statut == "Planifie");
                ViewBag.MesConsultations = await _context.Consultations
                    .CountAsync(c => c.MedecinId == medecin.Id);
                ViewBag.MesOrdonnances = await _context.Ordonnances
                    .CountAsync(o => o.Consultation.MedecinId == medecin.Id);
            }
            else
            {
                ViewBag.MesRDVAujourdhui = 0;
                ViewBag.MesConsultations = 0;
                ViewBag.MesOrdonnances = 0;
            }

            return View();
        }

        public async Task<IActionResult> Patient()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var patient = user != null ? await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id) : null;

            if (patient != null)
            {
                ViewBag.MesRDVProchains = await _context.RendezVous
                    .CountAsync(r => r.PatientId == patient.Id && r.DateRdv >= DateTime.Today && r.Statut == "Planifie");
                ViewBag.MesFacturesEnAttente = await _context.Factures
                    .CountAsync(f => f.PatientId == patient.Id && f.Statut == "NonPayee");

                var dossier = await _context.DossierMedicals.FirstOrDefaultAsync(d => d.PatientId == patient.Id);
                if (dossier != null)
                {
                    ViewBag.MesOrdonnances = await _context.Ordonnances
                        .CountAsync(o => o.Consultation.DossierMedicalId == dossier.Id);
                }
                else
                {
                    ViewBag.MesOrdonnances = 0;
                }
            }
            else
            {
                ViewBag.MesRDVProchains = 0;
                ViewBag.MesFacturesEnAttente = 0;
                ViewBag.MesOrdonnances = 0;
            }

            return View();
        }
    }
}
