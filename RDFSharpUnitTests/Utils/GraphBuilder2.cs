using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDFSharpTests.Utils
{
    public static class GraphBuilder2
    {
        public static readonly RDFResource dogOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "dogOf");
        public static readonly RDFResource catOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "catOf");
        public static readonly RDFResource isPetOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "isPetOf");
        //public static readonly RDFResource friendOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "friendOf");
        public static readonly RDFResource girlFriendOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "girlFriendOf");
        public static readonly RDFResource boyFriendOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "boyFriendOf");
        public static readonly RDFResource sisterOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "sisterOf");

        public static readonly RDFResource donaldDuck = new RDFResource("https://en.wikipedia.org/wiki/Donald_Duck");
        public static readonly RDFResource daisyDuck = new RDFResource("https://en.wikipedia.org/wiki/Daisy_Duck");

        // Della Duck (called Dumbella in Donald's Nephews; born 1920) is the mother of Huey, Dewey, and Louie. 
        // She was first described as Donald's cousin,[22] but was later referred to as Donald's twin sister
        public static readonly RDFResource dellaDuck = new RDFResource("https://en.wikipedia.org/wiki/Duck_family_(Disney)#Della_Duck");

        public static readonly RDFResource mickeyMouse = new RDFResource("https://en.wikipedia.org/wiki/Mickey_Mouse");
        // She is Mickey's female counterpart, an anthropomorphic mouse usually portrayed as his girlfriend 
        public static readonly RDFResource minnieMouse = new RDFResource("https://en.wikipedia.org/wiki/Minnie_Mouse");

        // Goofy is a close friend of Mickey Mouse and Donald Duck
        public static readonly RDFResource goofy = new RDFResource("https://en.wikipedia.org/wiki/Goofy");

        // Figaro is probably best known as the pet cat of Mister Geppetto and Pinocchio. 
        // Figaro was Walt Disney's favorite character in Pinocchio; he loved the kitten so much, he wanted him to appear as much as possible. 
        // Meanwhile, he was now Minnie's pet.  
        public static readonly RDFResource figaro = new RDFResource("https://en.wikipedia.org/wiki/Figaro_(Disney)");

        // Pluto is Mickey Mouse's pet
        public static readonly RDFResource pluto = new RDFResource("https://en.wikipedia.org/wiki/Pluto_(Disney)");


        public static RDFGraph WaltDisneyGraphBuild()
        {
            RDFGraph waltdisney = new RDFGraph();
            waltdisney.SetContext(new Uri("http://waltdisney.com/"));

            // https://en.wikipedia.org/wiki/Mickey_Mouse_universe#Minnie_Mouse_family

            // Create triples

            // "Mickey Mouse is 85 years old"
            RDFTriple mickeyMouse_is85yr = new RDFTriple(mickeyMouse, RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER));
            RDFTriple mickeyMouse_isMale = new RDFTriple(mickeyMouse, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Male", "en"));
            RDFTriple mickeyMouseNameEn = new RDFTriple(mickeyMouse, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Mickey Mouse", "en"));
            RDFTriple mickeyMouseFamilyNameEn = new RDFTriple(mickeyMouse, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Mouse", "en"));
            RDFTriple mickeyMouse_boyFriendOf_MinnieMouse = new RDFTriple(mickeyMouse, boyFriendOf, minnieMouse);

            List<RDFTriple> mickeyMouseTriples = new List<RDFTriple> { mickeyMouse_is85yr, mickeyMouse_isMale, mickeyMouseNameEn, mickeyMouseFamilyNameEn, mickeyMouse_boyFriendOf_MinnieMouse };

            RDFGraph mickeyMouseGraph = new RDFGraph(mickeyMouseTriples);
            foreach (var triple in mickeyMouseGraph)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple minnieMouse_is82yr = new RDFTriple(minnieMouse, RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("82", RDFModelEnums.RDFDatatypes.XSD_INTEGER));
            RDFTriple minnieMouse_isFemale = new RDFTriple(minnieMouse, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Female", "en"));
            RDFTriple minnieMouseNameEn = new RDFTriple(minnieMouse, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Minnie Mouse", "en"));
            RDFTriple minnieMouseFamilyNameEn = new RDFTriple(minnieMouse, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Mouse", "en"));
            RDFTriple minnieMouseGirlFriendOfMickeyMouse = new RDFTriple(minnieMouse, girlFriendOf, mickeyMouse);

            List<RDFTriple> minnieMouseTriples = new List<RDFTriple> { minnieMouse_is82yr, minnieMouse_isFemale, minnieMouseNameEn, minnieMouseFamilyNameEn, minnieMouseGirlFriendOfMickeyMouse };
            foreach (var triple in minnieMouseTriples)
            {
                waltdisney.AddTriple(triple);
            }

            // Figaro is based on and acts like an immature and spoiled little boy.
            RDFTriple figaro_isMale = new RDFTriple(figaro, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Male", "en"));
            RDFTriple figaroNameEn = new RDFTriple(figaro, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Figaro", "en"));
            // Figaro is now Minnie's pet.
            RDFTriple figaroCatOfMinnieMouse = new RDFTriple(figaro, catOf, minnieMouse);
            RDFTriple figaroIsPetOfMinnieMouse = new RDFTriple(figaro, isPetOf, minnieMouse);

            List<RDFTriple> figaroTriples = new List<RDFTriple> { figaro_isMale, figaroNameEn, figaroCatOfMinnieMouse, figaroIsPetOfMinnieMouse };
            foreach (var triple in figaroTriples)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple pluto_isMale = new RDFTriple(pluto, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Male", "en"));
            RDFTriple plutoNameEn = new RDFTriple(pluto, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Pluto", "en"));
            // Pluto is Mickey Mouse's pet
            RDFTriple plutoDogOfMickeyMouse = new RDFTriple(pluto, dogOf, mickeyMouse);
            RDFTriple plutoIsPetOfMickeyMouse = new RDFTriple(pluto, isPetOf, mickeyMouse);

            List<RDFTriple> plutoTriples = new List<RDFTriple> { pluto_isMale, plutoNameEn, plutoDogOfMickeyMouse, plutoIsPetOfMickeyMouse };
            foreach (var triple in plutoTriples)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple goofy_isMale = new RDFTriple(goofy, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Male", "en"));
            RDFTriple goofyNameEn = new RDFTriple(goofy, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Goofy Goof", "en"));
            RDFTriple goofyFamilyNameEn = new RDFTriple(goofy, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Goof", "en"));
            // Goofy is a close friend of Mickey Mouse and Donald Duck
            RDFTriple goofyFriendOfMickeyMouse = new RDFTriple(goofy, RDFVocabulary.FOAF.KNOWS, mickeyMouse);
            RDFTriple goofyFriendOfDonaldDuck = new RDFTriple(goofy, RDFVocabulary.FOAF.KNOWS, donaldDuck);

            List<RDFTriple> goofyTriples = new List<RDFTriple> { goofy_isMale, goofyNameEn, goofyFamilyNameEn, goofyFriendOfMickeyMouse, goofyFriendOfDonaldDuck };
            foreach (var triple in goofyTriples)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple donaldDuckNameEn = new RDFTriple(donaldDuck, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Donald Duck", "en"));
            // "Donald Duck is 85 years old"
            RDFTriple donaldDuck_is85yr = new RDFTriple(donaldDuck, RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER));
            RDFTriple donaldDuck_isMale = new RDFTriple(donaldDuck, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Male", "en"));
            RDFTriple donaldDuckFamilyNameEn = new RDFTriple(donaldDuck, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Duck", "en"));
            RDFTriple donaldDuck_boyFriendOf_daisyDuck = new RDFTriple(donaldDuck, boyFriendOf, daisyDuck);

            List<RDFTriple> donaldDuckTriples = new List<RDFTriple> { donaldDuckNameEn, donaldDuck_is85yr, donaldDuck_isMale, donaldDuckFamilyNameEn, donaldDuck_boyFriendOf_daisyDuck };
            foreach (var triple in donaldDuckTriples)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple daisyDuckNameEn = new RDFTriple(daisyDuck, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Daisy Duck", "en"));
            RDFTriple daisyDuck_isFemale = new RDFTriple(daisyDuck, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Female", "en"));
            RDFTriple daisyDuckFamilyNameEn = new RDFTriple(daisyDuck, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Duck", "en"));
            RDFTriple daisyDuckGirlFriendOfDonaldDuck = new RDFTriple(daisyDuck, girlFriendOf, donaldDuck);

            List<RDFTriple> daisyDuckTriples = new List<RDFTriple> { daisyDuckNameEn, daisyDuck_isFemale, daisyDuckFamilyNameEn, daisyDuckGirlFriendOfDonaldDuck };
            foreach (var triple in daisyDuckTriples)
            {
                waltdisney.AddTriple(triple);
            }

            RDFTriple dellaDuckNameEn = new RDFTriple(dellaDuck, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Della Duck", "en"));
            RDFTriple dellaDuck_isFemale = new RDFTriple(dellaDuck, RDFVocabulary.FOAF.GENDER, new RDFPlainLiteral("Female", "en"));
            RDFTriple dellaDuckFamilyNameEn = new RDFTriple(dellaDuck, RDFVocabulary.FOAF.FAMILY_NAME, new RDFPlainLiteral("Duck", "en"));
            RDFTriple dellaDuckSisterOfDonaldDuck = new RDFTriple(dellaDuck, sisterOf, donaldDuck);

            List<RDFTriple> dellaDuckTriples = new List<RDFTriple> { dellaDuckNameEn, dellaDuck_isFemale, dellaDuckFamilyNameEn, dellaDuckSisterOfDonaldDuck };
            foreach (var triple in dellaDuckTriples)
            {
                waltdisney.AddTriple(triple);
            }

            return waltdisney;
        }
    }
}
