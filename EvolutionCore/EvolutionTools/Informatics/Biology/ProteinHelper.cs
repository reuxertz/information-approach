using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class ProteinHelper
    {
        public static string AminoAcidCharSingleString = "ARNDCQEGHILKMFPSTWYV";
        public static char[] AminoAcidChars = new char[] { 
            'A', 'R', 'N', 'D', 'C', 
            'Q', 'E', 'G', 'H', 'I', 
            'L', 'K', 'M', 'F', 'P', 
            'S', 'T', 'W', 'Y', 'V' };
        public static string[] AminoAcidCharStrings = new string[] { 
            "A", "R", "N", "D", "C", 
            "Q", "E", "G", "H", "I", 
            "L", "K", "M", "F", "P", 
            "S", "T", "W", "Y", "V" };
        public static string[] AminoAcid3CharStrings = new string[] {
            "ala", "arg", "asn", "asp", "cys", 
            "gln", "glu", "gly", "his", "ile", 
            "leu", "lys", "met", "phe", "pro", 
            "ser", "thr", "trp", "tyr", "val" };
        public static string[] AminoAcidNameStrings = new string[] {
            "alanine", "arginine", "asparagine", "aspartate", "cysteine", 
            "glutamine", "glutamate", "glycine", "histidine", "isoleucine", 
            "leucine", "lysine", "methionine", "phenylalanine", "proline", 
            "serine", "threonine", "tryptophan", "tyrosine", "valine" };
        public static float[] AminoAcidProbabilities = new float[] { 
            .074f, .042f, .044f, .059f, .033f, 
            .037f, .058f, .074f, .029f, .038f, 
            .076f, .072f, .018f, .040f, .050f, 
            .081f, .062f, .013f, .033f, .068f };
    }
}
