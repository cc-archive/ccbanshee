/***************************************************************************
 *  TrackInfo.cs
 *
 *  Copyright (C) 2005-2006 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using Mono.Unix;
using Gtk;
 
namespace Banshee.Base
{
    public enum RemoteLookupStatus {
        NoAttempt = 0,
        Success,
        Failure
    }
    
    public enum LicenseVerifyStatus {
        NoAttempt = 0,
        Valid,
        Invalid
    }
    
    public class HaveTrackInfoArgs : EventArgs
    {
        public TrackInfo TrackInfo;
    }

    public delegate void HaveTrackInfoHandler(object o, HaveTrackInfoArgs args);
    
    public abstract class TrackInfo
    {
        protected SafeUri uri;
        protected string mimetype;
        protected string artist;
        protected string album;
        protected string title;
        protected DateTime date_added;
        protected int year;
        protected string genre;
        protected string performer;

        protected Licenses.Attributes license = Licenses.Attributes.none;   // None if unverified
        protected string copyright;     // License and Verification URIs (TCOP)
        protected string license_uri;   // License URI (WCOP)
        protected string metadata_uri;  // Metadata verification URI (WOAF)

        protected string asin;
        protected string label;

        protected uint rating;
        protected uint play_count;
        protected DateTime last_played;

        protected TimeSpan duration;
        protected uint track_number;
        protected uint track_count;
        protected int track_id;

        protected double track_gain;
        protected double track_peak;
        protected double album_gain;
        protected double album_peak;

        protected bool can_save_to_database;
        protected bool can_play = true;
        protected RemoteLookupStatus remote_lookup_status = RemoteLookupStatus.NoAttempt;
        protected LicenseVerifyStatus license_verify_status = LicenseVerifyStatus.NoAttempt;

        public Gtk.TreeIter PreviousTrack;
        public Gtk.TreeIter TreeIter;
        
        private int uid;
        
        private static int next_id = 1;

        public event EventHandler Changed;
        
        protected TrackInfo()
        {
            uid = next_id++;
        }
        
        public virtual void Save()
        {
        }

        public virtual void IncrementPlayCount()
        {
        }

        protected virtual void SaveRating()
        {
        }

        protected virtual void WriteUpdate()
        {
        }

        protected virtual void OnChanged()
        {
            EventHandler handler = Changed;
            if(handler != null) {
                handler(this, new EventArgs());
            }
        }

        public int TrackId { 
            get { 
                return track_id;
            }
            
            protected set {
                track_id = value;
            }
        }
        
        public int Uid { 
            get { 
                return uid;
            }
        }

        public SafeUri Uri { 
            get { 
                return uri;
            }
            
            set { 
                uri = value; WriteUpdate();
            } 
        }
       
        public string Asin { 
            get { 
                return asin;
            }
            
            set { 
                asin = value;
            } 
        }
        
        public RemoteLookupStatus RemoteLookupStatus {
            get {
                return remote_lookup_status;
            }
            
            set {
                remote_lookup_status = value;
            }
        }
        
        public LicenseVerifyStatus LicenseVerifyStatus {
            get {
                return license_verify_status;
            }
            
            set {
                license_verify_status = value;
            }
        }
        
        protected string cover_art_file = null;
        public string CoverArtFileName { 
            get {
                if(cover_art_file != null) {
                    return cover_art_file;
                }
                
                string path = Paths.GetCoverArtPath(asin);
                if(File.Exists(path)) {
                    cover_art_file = path;
                    return path;
                }
               
                string basepath = Path.GetDirectoryName(Uri.AbsolutePath) + Path.DirectorySeparatorChar;
               
                path = basepath + Asin + ".01._SCLZZZZZZZ_.jpg";
                if(File.Exists(path)) { 
                    cover_art_file = path;
                    return path;
                }
               
                string [] cover_names = { "cover", "Cover", "folder", "Folder" };
                string [] cover_extensions = { "jpg", "png", "jpeg", "gif" };
                
                foreach(string cover in cover_names) {
                    foreach(string ext in cover_extensions) {
                        string img = basepath + cover + "." + ext;
                        if(File.Exists(img)) {
                            cover_art_file = img;
                            return img;
                        }
                    }
                }
                
                return null;
            }
        }
       
        [BansheeTodo("MimeType should be parsed using gvfs and set accordingly")]
        public string MimeType { 
            get { 
                return mimetype;
            } 
            
            set {
                mimetype = value;
            }
        }
        
        public string Artist { 
            get { 
                return artist;
            }
            
            set { 
                artist = value; 
                WriteUpdate();   
            } 
        }
        
        public string Album { 
            get { 
                return album;
            }
            
            set { 
                album = value; 
                WriteUpdate();
            } 
        }
        
        public string Title { 
            get { 
                return title;
            }
            
            set { 
                title = value; 
                WriteUpdate();
            } 
        }
        
        [BansheeTodo("Need to figure out how we want to display this")]
        public string Genre { 
            get { 
                return genre;
            } 
            
            set { 
                genre = value; 
                WriteUpdate(); 
            } 
        }

        public Licenses.Attributes License {
            get {
                return license;
            }

            set {
                license = value;
                WriteUpdate();
            }
        }

        public string Copyright { 
            get { 
                return copyright;
            }

            set { 
                copyright = value;
                WriteUpdate();
            } 
        }

        public string LicenseUri { 
            get { 
                return license_uri;
            }

            set { 
                license_uri = value;
                WriteUpdate();
            } 
        }

        public string MetadataUri {
            get {
                return metadata_uri;
            }

            set {
                metadata_uri = value;
                WriteUpdate();
            }
        }

        [BansheeTodo("Completely unused, should we even have this?")]
        public string Performer {
            get { 
                return performer; 
            } 
            
            set {
                performer = value;
                WriteUpdate();
            }
        }
        
        public int Year { 
            get { 
                return year;
            }
            
            set { 
                year = value; 
                WriteUpdate(); 
            }
        }

        public TimeSpan Duration {
            get { 
                return duration;
            }
            
            set { 
                duration = value; 
                WriteUpdate(); 
            } 
        }

        public uint TrackNumber { 
            get { 
                return track_number;
            } 
            
            set { 
                track_number = value; 
                WriteUpdate();
            }
        }
        
        public string TrackNumberTitle {
            get {
                return String.Format ("{0:00} - {1}", track_number, title);
            }
        }
        
        public uint TrackCount { 
            get { 
                return track_count;
            }
            
            set { 
                track_count = value; 
                WriteUpdate();
            }
        }

        public uint PlayCount { 
            get { 
                return play_count;
            }
        }
        
        public DateTime LastPlayed { 
            get { 
                return last_played;
            }
        }
        
        public DateTime DateAdded { 
            get { 
                return date_added;
            }
        }

        public uint Rating { 
            get { 
                return rating;
            }
            
            set { 
                rating = value; 
                WriteUpdate(); 
                SaveRating();
            }
        }
        
        [BansheeTodo("Placeholder for ReplayGain support")]
        public double TrackGain { 
            get { 
                return track_gain;
            }
        }
        
        [BansheeTodo("Placeholder for ReplayGain support")]
        public double TrackPeak { 
            get { 
                return track_peak;
            }
        }
        
        [BansheeTodo("Placeholder for ReplayGain support")]
        public double AlbumGain { 
            get { 
                return album_gain;
            }
        }
        
        [BansheeTodo("Placeholder for ReplayGain support")]
        public double AlbumPeak { 
            get { 
                return album_peak;
            }
        }

        public string DisplayArtist { 
            get { 
                return artist == null || artist == String.Empty
                    ? Catalog.GetString("Unknown Artist") 
                    : artist; 
            } 
        }

        public string DisplayAlbum { 
            get { 
                return album == null || album == String.Empty 
                    ? Catalog.GetString("Unknown Album") 
                    : album; 
            } 
       }

        public string DisplayTitle { 
            get { 
                return title == null || title == String.Empty
                    ? Catalog.GetString("Unknown Title") 
                    : title; 
            } 
        }        

        public bool CanSaveToDatabase {
            get { 
                return can_save_to_database; 
            }
            
            protected set {
                can_save_to_database = value;
            }
        }

        public virtual bool CanPlay {
            get { 
                return can_play; 
            }
            
            protected set {
                can_play = value;
            }
        }

        public override string ToString()
        {
            return String.Format ("{0} - {1} - {2} ({3})", Artist, Album, Title, Uri.AbsoluteUri);
        }
        
        public override bool Equals(object o)
        {
            if(!(o is TrackInfo)) {
                return false;
            }
            
            if(o == this) {
                return true;
            }
            
            TrackInfo t = o as TrackInfo;
            return t.Artist == Artist && t.Album == Album && t.Title == Title;
        }
        
        public override int GetHashCode()
        {
            return Artist.GetHashCode() ^ Album.GetHashCode() ^ Title.GetHashCode();
        }
    }
}
