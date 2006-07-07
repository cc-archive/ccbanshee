/***************************************************************************
 *  CreativeCommons.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using System;
using System.IO;
using System.Web;
using System.Net;
using System.Xml;
using System.Text;
using System.Security.Cryptography;
using Banshee.Base;
using CCLicenseLib.Utilities;

namespace Banshee.Base
{
    public struct TrackLicenseClaim
    {
        public string TrackPath;
        public string LicenseUri;
        public string MetadataUri;
       
        public override string ToString()
        {
            return String.Format("Under \"{0}\" verify at \"{1}\"", LicenseUri, MetadataUri);
        }
    }

	public static class CreativeCommons
	{
	    private const string HTTP_STRING = "http://";
		private const string VERIFY_STRING = "verify at "; // language that denotes where to verify license
		private const int VERIFY_LENGTH = 10;              // length to skip to get the uri

		public static string VerifyLicense(TrackInfo track)
		{
	        // Get License and metadata URIs
	        TrackLicenseClaim claim;
	        try {
                claim = BuildClaim(track);
	        } catch(ArgumentException e) {
                Console.WriteLine(e);
                return null;    
	        }

            // Get CC RDF metadata
			RDFParser metadata_parser = new RDFParser(claim.MetadataUri, true);
			// Get Metadata's License Uri
			Console.WriteLine("Getting Metadata's License URI");
			string metadata_license_uri = GetMetadataLicenseURI(metadata_parser.GetRDFAsString(), claim);
			
			// Check Claim
			Console.WriteLine("Checking Claim");
			if(metadata_license_uri == claim.LicenseUri) {
			    return claim.LicenseUri;
			}
			// Check if claim.LicenseUri == metadata_license_uri
			/*if(license_hash == metadata_hash) {
				return GetLicenseName(metadata_parser.GetRDFAsString());
			} else {
				// Use Banshee log to record the error
				return "False Claim";
			}*/
			return "hey";
		}
		
		// Get License and metadata URIs
		private static TrackLicenseClaim BuildClaim(TrackInfo track)
		{
		    TrackLicenseClaim claim = new TrackLicenseClaim();

	    	if((track.LicenseUri != null) &&
			   (track.MetadataUri != null)) { // WCOP/WOAF method
			    Console.WriteLine("== WCOP ==");
				claim.LicenseUri = track.LicenseUri;
			    claim.MetadataUri = track.MetadataUri;
			} else if(track.Copyright != null) { // TCOP method (depricated)
			    Console.WriteLine("== TCOP ==");
				claim.LicenseUri = ParseLicenseURI(track.Copyright);
			    claim.MetadataUri = ParseMetadataURI(track.Copyright);
			} else { // No claim
				throw new ArgumentException("No license claim was made.");
			}
			
			claim.TrackPath = track.Uri.AbsolutePath;
			
			return claim;
		}
		
		// TODO: Is it better to do all the parsing here or in steps of regex and xmltextreader?
		// Extract the license URI from the rdf tag
		private static string GetMetadataLicenseURI(string rdf, TrackLicenseClaim claim)
		{
			try {
			    // Parse the RDF
                XmlTextReader reader = new XmlTextReader(new StringReader(rdf));
                
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                XmlNode docElement = doc.DocumentElement;
                
                // Hash the media file
                Console.WriteLine("Making Hasher");
                SHA1Managed hasher = new SHA1Managed();
                Console.WriteLine("Opening file: \"{0}\"", claim.TrackPath);
                Stream track_stream = File.OpenRead(claim.TrackPath);
                Console.WriteLine("Hashing track");
                byte[] track_hash_bytes = hasher.ComputeHash(track_stream);
                Console.WriteLine("Making String from hash");                
                string track_hash = MakeString(track_hash_bytes);
                Console.WriteLine("Track Hash: \"{0}\"", track_hash);
                
                // Look for rdf:about  uri_hash = HashUri(claim.TrackPath)
                XmlNodeList result = docElement.SelectNodes("/rdf:RDF/Work/text()");
                foreach(XmlNode n in result) {
                    if(n.Value == track_hash) {
                        return "found";
                    }
                }
                
                return "laksjflaskdjfls";
            } catch (XmlException e) {
                Console.WriteLine("error occured: " + e.Message);
                return null; // TODO: This might not be what I want!
            }
		}
		
		private static string MakeString(byte[] characters)
        {
            UnicodeEncoding encoding = new UnicodeEncoding( );
            string constructedString = encoding.GetString(characters);

            return (constructedString);
        }
		
		// Return a license name based on the license URI using CCLicenseLib
		private static string GetLicenseName(string rdf)
		{
			return null;
		}

        // TODO: VERIFY_STRING is being indexed twice. This is bad.
		// Extract the license URI from TCOP frame	
		private static string ParseLicenseURI(string data)
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
		
		// Extract the metadata URI from TCOP frame data
		private static string ParseMetadataURI(string data)
		{
			int verify_index = data.ToLower().IndexOf(VERIFY_STRING);
			if(verify_index <= 0) {
			    throw new ArgumentException("No metadata was found in Copyright tag at parsing metadata url");
			}

			return data.Substring(verify_index + VERIFY_LENGTH, data.Length - (verify_index + VERIFY_LENGTH));
		}
	}
}
