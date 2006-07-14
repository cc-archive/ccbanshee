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
        		
        private static string GetLicenseName(string rdf)
        {
            return "Valid";
        }
	}
	
	public sealed class LicenseUri 
    {
        public const string Title             = "title";
        public const string Artist            = "artist";
        public const string Album             = "album";
        public const string Date              = "date";
        public const string Genre             = "genre";
        public const string Comment           = "comment";
        public const string TrackNumber       = "track-number";
        public const string TrackCount        = "track-count";
        public const string AlbumVolumeNumber = "album-disc-number";
        public const string AlbumVolumeCount  = "album-disc-count";
        public const string Location          = "location";
        public const string Description       = "description";
        public const string Version           = "version";
        public const string Isrc              = "isrc";
        public const string Organization      = "organization";
        public const string Copyright         = "copyright";
        public const string Contact           = "contact";
        public const string License           = "license";
        public const string Performer         = "performer";
        public const string Duration          = "duration";
        public const string Codec             = "codec";
        public const string VideoCodec        = "video-codec";
        public const string AudioCodec        = "audio-codec";
        public const string Bitrate           = "bitrate";
        public const string NominalBitrate    = "nominal-bitrate";
        public const string MinimumBitrate    = "minimum-bitrate";
        public const string MaximumBitrate    = "maximum-bitrate";
        public const string Serial            = "serial";
        public const string Encoder           = "encoder";
        public const string EncoderVersion    = "encoder-version";
        public const string TrackGain         = "replaygain-track-gain";
        public const string TrackPeak         = "replaygain-track-peak";
        public const string AlbumGain         = "replaygain-album-gain";
        public const string AlbumPeak         = "replaygain-album-peak";
        public const string StreamType        = "stream-type";
    }
}
