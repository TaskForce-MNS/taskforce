import { LegalPageTemplate } from '@/components/profile/LegalPageTemplate';

export const MentionsLegales = () => {
  return (
    <LegalPageTemplate title="Mentions Légales" lastUpdated="29 septembre 2026">
      <h2>1. Éditeur du site</h2>
      <p>
        Le site <strong>TaskForce</strong> est édité par Sutcu,
        auto-entrepreneur au capital de 5 000 €,
        immatriculée au Registre du Commerce et des Sociétés de Metz sous le numéro [Numéro SIRET].
      </p>
      <p>
        <strong>Siège social :</strong> Metz<br />
        <strong>Directeur de la publication :</strong> Sutcu<br />
        <strong>Contact :</strong> selimsutcu@proton.me
      </p>

      <h2>2. Hébergement</h2>
      <p>
        Ce site est hébergé par <strong>MNS</strong>.<br />
        <strong>Siège social de l'hébergeur :</strong> Metz<br />
        <strong>Site web :</strong> metznumeriqueschool.fr
      </p>

      <h2>3. Propriété intellectuelle</h2>
      <p>
        L'ensemble de ce site relève de la législation française et internationale sur le droit d'auteur
        et la propriété intellectuelle. Tous les droits de reproduction sont réservés, y compris pour
        les documents téléchargeables et les représentations iconographiques et photographiques.
      </p>

      <h2>4. Données personnelles (RGPD)</h2>
      <p>
        Les informations recueillies font l'objet d'un traitement informatique destiné à la gestion de
        l'application TaskForce (gestion des projets, des tâches et des utilisateurs).
        Conformément à la loi "informatique et libertés" du 6 janvier 1978 modifiée, vous bénéficiez
        d'un droit d'accès et de rectification aux informations qui vous concernent.
      </p>
      <p>
        Pour exercer ce droit, veuillez nous contacter à l'adresse suivante : selimsutcu@proton.me.
      </p>
    </LegalPageTemplate>
  );
};