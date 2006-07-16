/***************************************************************************
 *  CreativeCommons.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Security.Cryptography;
using CCLicenseLib.Utilities;

namespace Banshee.Base
{
    public static class CreativeCommons
    {
        private const string HTTP_STRING = "http://";
        private const string VERIFY_STRING = "verify at ";
        private const int VERIFY_LENGTH = 10;
        
        public static void VerifyLicense(TrackInfo track)
        {
            try {
                /* Check for a full license claim */
                CheckLicenseClaim(track);
                
                /* Parse RDF metadata from verification URL in license claim */
                RDFParser metadata_parser = new RDFParser(track.MetadataUri, true);
                
                /* Look for license in RDF metadata */
                string license_name = FindLicenseInMetadata(track.LicenseUri,
                                                            HashData(track.Uri.AbsolutePath),
                                                            metadata_parser.GetRDFAsString());
                
                /* Set license name */
                track.License = license_name;
            } catch(Exception e) {
                // TODO: Integrate with LogCore
                Console.WriteLine(e);
            }
        }
        
        private static void CheckLicenseClaim(TrackInfo track)
        {
            if(track.Copyright == null) {
                if(track.LicenseUri == null || track.MetadataUri == null) {
                    throw new Exception("Track contains no claim.");
                }
            } else {
                track.LicenseUri = ParseLicenseUri(track.Copyright);
                track.MetadataUri = ParseMetadataUri(track.Copyright);
            }
        }
        
        // TODO: VERIFY_STRING is being searched twice.
        private static string ParseLicenseUri(string data)
        {
            int verify_index = (data.ToLower()).IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new Exception("No metadata was found in Copyright tag while parsing license URL.");    
            }
            
            int http_index = (data.ToLower()).LastIndexOf(HTTP_STRING, verify_index);
            if(http_index <= 0) {
                throw new Exception("No license was found in Copyright tag.");
            }
            
            return data.Substring(http_index, (verify_index - http_index) - 1);
        }

        private static string ParseMetadataUri(string data)
        {
            int verify_index = data.ToLower().IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new Exception("No metadata was found in Copyright tag at parsing metadata url");
            }

            return data.Substring(verify_index + VERIFY_LENGTH, data.Length - (verify_index + VERIFY_LENGTH));
        }
        
        private static string HashData(string file_path) {
            SHA1Managed hasher = new SHA1Managed();
            Base32 b32 = new Base32(hasher.ComputeHash(File.OpenRead(file_path)));
            string file_hash = b32.ToString();
            Console.WriteLine("File: \"{0}\" Hash: \"{1}\"", file_path, file_hash);
            return file_hash;
        }

        private static string FindLicenseInMetadata(string license_uri, string track_hash, string metadata)
        {
            XPathDocument doc = new XPathDocument(new StringReader(metadata));
       	    XPathNavigator navigator = doc.CreateNavigator();
       	    XPathExpression expression = navigator.Compile(
                String.Format("/rdf:RDF/r:Work[@rdf:about='urn:sha1:{0}']/r:license/@rdf:resource", track_hash));

       	    XmlNamespaceManager namespaces = new XmlNamespaceManager(new NameTable());
       	    namespaces.AddNamespace ("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
       	    namespaces.AddNamespace ("dc", "http://purl.org/dc/elements/1.1/");
       	    namespaces.AddNamespace ("r", "http://web.resource.org/cc/");
       	    expression.SetContext(namespaces);

       	    XPathNodeIterator it = navigator.Select(expression);
       	    Console.WriteLine("Found {0} license match(es) in metadata.", it.Count);
       	    while (it.MoveNext()) {
       	        XPathNavigator n = it.Current;
       	        if(n.Value == license_uri) {
       	            return GetLicenseName(license_uri);
       	        }
       	    }
       	    throw new Exception("License was not found in RDF metadata.");
        }
        
        private static string GetLicenseName(string license_uri)
        {
            string license_name;
            switch(license_uri.ToLower()) {
                case LicenseUri.Attribution:
                    license_name = LicenseName.Attribution;
                    break;
                case LicenseUri.Attribution_NoDerivs:
                    license_name = LicenseName.Attribution_NoDerivs;
                    break;
                case LicenseUri.Attribution_NonCommercial_NoDerivs:
                    license_name = LicenseName.Attribution_NonCommercial_NoDerivs;
                    break;
                case LicenseUri.Attribution_NonCommercial:
                    license_name = LicenseName.Attribution_NonCommercial;
                    break;
                case LicenseUri.Attribution_NonCommercial_ShareAlike:
                    license_name = LicenseName.Attribution_NonCommercial_ShareAlike;
                    break;
                case LicenseUri.Attribution_ShareAlike:
                    license_name = LicenseName.Attribution_ShareAlike;
                    break;
                case LicenseUri.NoDerivs:
                    license_name = LicenseName.NoDerivs;
                    break;
                case LicenseUri.NoDerivs_NonCommercial:
                    license_name = LicenseName.NoDerivs_NonCommercial;
                    break;
                case LicenseUri.NonCommercial:
                    license_name = LicenseName.NonCommercial;
                    break;
                case LicenseUri.NonCommercial_ShareAlike:
                    license_name = LicenseName.NonCommercial_ShareAlike;
                    break;
                case LicenseUri.ShareAlike:
                    license_name = LicenseName.ShareAlike;
                    break;
                default:
                    throw new ArgumentException("Invalid license URL.");
            }
            return license_name;
        }
	}
	
	public sealed class LicenseUri
	{
	    // Version 2.5 Licenses
        public const string Attribution                             = "http://creativecommons.org/licenses/by/2.5/";
        public const string Attribution_NoDerivs                    = "http://creativecommons.org/licenses/by-nd/2.5/";
        public const string Attribution_NonCommercial_NoDerivs      = "http://creativecommons.org/licenses/by-nc-nd/2.5/";
        public const string Attribution_NonCommercial               = "http://creativecommons.org/licenses/by-nc/2.5/";
        public const string Attribution_NonCommercial_ShareAlike    = "http://creativecommons.org/licenses/by-nc-sa/2.5/";
        public const string Attribution_ShareAlike                  = "http://creativecommons.org/licenses/by-sa/2.5/";
        // Version 1.0 Licenses
        public const string NoDerivs                                = "http://creativecommons.org/licenses/nd/1.0/";
        public const string NoDerivs_NonCommercial                  = "http://creativecommons.org/licenses/nd-nc/1.0/";
        public const string NonCommercial                           = "http://creativecommons.org/licenses/nc/1.0/";
        public const string NonCommercial_ShareAlike                = "http://creativecommons.org/licenses/nc-sa/1.0/";
        public const string ShareAlike                              = "http://creativecommons.org/licenses/sa/1.0/";
    }
    
    public sealed class LicenseName
	{
	    // Version 2.5 Licenses
        public const string Attribution                             = "Attribution";
        public const string Attribution_NoDerivs                    = "Attribution-NoDerivs";
        public const string Attribution_NonCommercial_NoDerivs      = "Attribution-NonCommercial-NoDerivs";
        public const string Attribution_NonCommercial               = "Attribution-NonCommercial";
        public const string Attribution_NonCommercial_ShareAlike    = "Attribution-NonCommercial-ShareAlike";
        public const string Attribution_ShareAlike                  = "Attribution-ShareAlike";
        // Version 1.0 Licenses
        public const string NoDerivs                                = "NoDerivs";
        public const string NoDerivs_NonCommercial                  = "NoDerivs-NonCommercial";
        public const string NonCommercial                           = "NonCommercial";
        public const string NonCommercial_ShareAlike                = "NonCommercial-ShareAlike";
        public const string ShareAlike                              = "ShareAlike";
    }
}
