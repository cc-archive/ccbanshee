/***************************************************************************
 *  LicenseRenderer.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using Gtk;
using Gdk;

using Banshee.Base;

namespace Banshee
{
	public class LicenseRenderer : CellRenderer
	{
	    private const int MAX_LICENSES = 3;
	    static private Pixbuf attribution;
	    static private Pixbuf no_derivs;
	    static private Pixbuf noncommercial;
		static private Pixbuf public_domain;	    
		static private Pixbuf share_alike;
		
		public TrackInfo Track;
		
		public static Pixbuf Attribution
		{
			get {
				if(attribution == null)
					attribution = Gdk.Pixbuf.LoadFromResource("cc-attribution.png");
					
				return attribution;
			}
		}
		
		public static Pixbuf NoDerivs
		{
			get {
				if(no_derivs == null)
					no_derivs = Gdk.Pixbuf.LoadFromResource("cc-no-derivs.png");
					
				return no_derivs;
			}
		}
		
		public static Pixbuf NonCommercial
		{
			get {
				if(noncommercial == null)
					noncommercial = Gdk.Pixbuf.LoadFromResource("cc-noncommercial.png");
					
				return noncommercial;
			}
		}
		
		public static Pixbuf PublicDomain
		{
			get {
				if(public_domain == null)
					public_domain = Gdk.Pixbuf.LoadFromResource("cc-public-domain.png");
					
				return public_domain;
			}
		}
		
		public static Pixbuf ShareAlike
		{
			get {
				if(share_alike == null)
					share_alike = Gdk.Pixbuf.LoadFromResource("cc-share-alike.png");
					
				return share_alike;
			}
		}		
		
		public LicenseRenderer()
		{
			
		}

		protected LicenseRenderer(System.IntPtr ptr) : base(ptr)
		{
		
		}

		private StateType RendererStateToWidgetState(CellRendererState flags)
		{
			StateType state = StateType.Normal;
			
			if((CellRendererState.Insensitive & flags).Equals(
				CellRendererState.Insensitive)) {
				state = StateType.Insensitive;
			} else if((CellRendererState.Selected & flags).Equals( 
				CellRendererState.Selected)) {
				state = StateType.Selected;
			}
			
			return state;
		}
		
		protected override void Render(Gdk.Drawable drawable, 
			Widget widget, Gdk.Rectangle background_area, 
			Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, 
			CellRendererState flags)
		{
			Gdk.Window window = drawable as Gdk.Window;
			StateType state = RendererStateToWidgetState(flags);
			
			DrawRating(window, widget, cell_area, state, flags);
		}
		
		public override void GetSize(Widget widget, ref Gdk.Rectangle cell_area, 
			out int x_offset, out int y_offset, out int width, out int height)
		{
			height = Attribution.Height + 2;
			width = (Attribution.Width * MAX_LICENSES) + 4;
			x_offset = 0;
			y_offset = 0;
		}
	
		private void DrawRating(Gdk.Window canvas, Gtk.Widget widget,
			Gdk.Rectangle area, StateType state, CellRendererState flags)
		{
			string license = Track.License;
		    Pixbuf[] display = new Pixbuf[MAX_LICENSES];
            switch(license) {
                case LicenseName.Attribution:
                    display[0] = Attribution;
                    break;
                case LicenseName.Attribution_NoDerivs:
                    display[0] = Attribution;
                    display[1] = NoDerivs;
                    break;
                case LicenseName.Attribution_NonCommercial_NoDerivs:
                    display[0] = Attribution;
                    display[1] = NonCommercial;
                    display[2] = NoDerivs;
                    break;
                case LicenseName.Attribution_NonCommercial:
                    display[0] = Attribution;
                    display[1] = NonCommercial;
                    break;
                case LicenseName.Attribution_NonCommercial_ShareAlike:
                    display[0] = Attribution;
                    display[1] = NonCommercial;
                    display[2] = ShareAlike;
                    break;
                case LicenseName.Attribution_ShareAlike:
                    display[0] = Attribution;
                    display[1] = ShareAlike;
                    break;
                case LicenseName.NoDerivs:
                    display[0] = NoDerivs;
                    break;
                case LicenseName.NoDerivs_NonCommercial:
                    display[0] = NoDerivs;
                    display[1] = NonCommercial;
                    break;
                case LicenseName.NonCommercial:
                    display[0] = NonCommercial;
                    break;
                case LicenseName.NonCommercial_ShareAlike:
                    display[0] = NonCommercial;
                    display[1] = ShareAlike;
                    break;
                case LicenseName.ShareAlike:
                    display[0] = ShareAlike;
                    break;
                default:
                    display[0] = null;
                    break;
            }
			
			for(int i = 0; i < MAX_LICENSES; i++) {
			    if(display[i] == null) {
			        break;
			    } else {
    				canvas.DrawPixbuf(widget.Style.TextGC(state), display[i], 0, 0,
    					area.X + (i * Attribution.Width) + 1, area.Y + 1, 
    					Attribution.Width, Attribution.Height,
    					RgbDither.None, 0, 0);
			    }
			} 
		}
	}
}
