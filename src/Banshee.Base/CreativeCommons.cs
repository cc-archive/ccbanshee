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
            string license_uri;
            string metadata_uri;

            try {
                ParseClaim(track, out license_uri, out metadata_uri);
            } catch(ArgumentException e) {
                Console.WriteLine(e);
                return;    
            }

            RDFParser metadata_parser = new RDFParser(metadata_uri, true);
            if(LicenseInMetadata(license_uri,
                                 HashData(track.Uri.AbsolutePath),
                                 metadata_parser.GetRDFAsString())) {
                SetLicense(track, license_uri);
            }
        }

        private static void SetLicense(TrackInfo track, string license_uri) {
            track.License = GetLicenseName(license_uri);
        }

        private static void ParseClaim(TrackInfo track, out string license_uri, out string metadata_uri)
        {
            /* WCOP/WOAF and TCOP parsing */
            if((track.LicenseUri != null) &&
               (track.MetadataUri != null)) {
                license_uri = track.LicenseUri;
                metadata_uri = track.MetadataUri;
            } else if(track.Copyright != null) {
                license_uri = ParseLicenseUri(track.Copyright);
                metadata_uri = ParseMetadataUri(track.Copyright);
            } else {
                throw new ArgumentException("No license claim was made.");
            }
        }

        // TODO: Buffer hashing and read base32.py for more info on optimization
        private static string HashData(string file_path) {
            SHA1Managed hasher = new SHA1Managed();
            Base32 b32 = new Base32(hasher.ComputeHash(File.OpenRead(file_path)));
            string file_hash = b32.ToString();
            Console.WriteLine("File: \"{0}\" Hash: \"{1}\"", file_path, file_hash);
            return file_hash;
        }

        private static bool LicenseInMetadata(string license_uri, string track_hash, string metadata)
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
       	            return true;
       	        }
       	    }
       	    return false;
        }

        // TODO: VERIFY_STRING is being searched twice.
        private static string ParseLicenseUri(string data)
        {
            int verify_index = (data.ToLower()).IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new ArgumentException("No metadata was found in Copyright tag at parsing license url");    
            }
            
            int http_index = (data.ToLower()).LastIndexOf(HTTP_STRING, verify_index);
            if(http_index <= 0) {
                throw new ArgumentException("No license was found in Copyright tag");
            }
            
            return data.Substring(http_index, (verify_index - http_index) - 1);
        }

        private static string ParseMetadataUri(string data)
        {
            int verify_index = data.ToLower().IndexOf(VERIFY_STRING);
            if(verify_index <= 0) {
                throw new ArgumentException("No metadata was found in Copyright tag at parsing metadata url");
            }

            return data.Substring(verify_index + VERIFY_LENGTH, data.Length - (verify_index + VERIFY_LENGTH));
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
                    license_name = "Valid";
                    break;
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
    
    // TODO: Should licenses be stored by name or some sort of ID number in the database?
//    public enum Licenses {
//        Attribution,
//        Attribution_NoDerivs,
//        Attribution_NonCommercial_NoDerivs,
//        Attribution_NonCommercial,
//        Attribution_NonCommercial_ShareAlike,
//        Attribution_ShareAlike,
//        NoDerivs,
//        NoDerivs_NonCommercial,
//        NonCommercial,
//        NonCommercial_ShareAlike,
//        ShareAlike
//    }
    
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
