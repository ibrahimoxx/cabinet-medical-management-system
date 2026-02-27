# 💰 RÉSUMÉ : SYSTÈME FACTURATION

**Ce qu'on fait :** Création factures (Secrétaire), consultation (Secrétaire/Patient), paiements multiples (Patient), suivi statut automatique.

**Ce qu'on utilise :**
- Modèles `Facture` (PatientId, ConsultationId, Montant, Statut) + `Paiement` (ModePaiement, Montant)
- Calcul automatique statut "Payee" si somme paiements >= montant facture
- Modes paiement : "EnLigne", "Espece"

**Où c'est :**
- `Models/Facture.cs`, `Paiement.cs`
- `Controllers/FacturesController.cs` (Create, MesFactures, Payer, Imprimer)
- `Views/Factures/Index.cshtml`, `MesFactures.cshtml`, `Payer.cshtml`, `Imprimer.cshtml`

**Flux principal :**
1. Secrétaire crée facture → Lie consultation, Statut "NonPayee"
2. Patient voit facture → Clique "Payer"
3. Paiement enregistré → Vérifie si total >= montant
4. Si oui → Statut "Payee" automatiquement

