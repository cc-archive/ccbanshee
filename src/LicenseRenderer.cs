/***************************************************************************
 *  LicenseRenderer.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/
 
using System;
using System.Collections.Generic;
using Gtk;
using Gdk;

using Banshee.Base;

namespace Banshee
{
	public class LicenseRenderer : CellRenderer
	{
	    private const int MAX_ATTRIBUTES = 3;
	    private static string[] split_result = new string[MAX_ATTRIBUTES];
	    private static char[] splitter  = {'-'};	    
	    
	    private static Dictionary<string, Gdk.Pixbuf> icons;	    
	    
		public TrackInfo Track;
		
		public LicenseRenderer()
		{
			BuildDictionary();
		}

		protected LicenseRenderer(System.IntPtr ptr) : base(ptr)
		{
            BuildDictionary();
		}
		
		private void BuildDictionary() {
		    if(icons == null) {
                icons = new Dictionary<string, Gdk.Pixbuf>();
                icons.Add("by", Gdk.Pixbuf.LoadFromResource("cc-attribution.png"));
                icons.Add("nd", Gdk.Pixbuf.LoadFromResource("cc-no-derivs.png"));
                icons.Add("nc", Gdk.Pixbuf.LoadFromResource("cc-noncommercial.png"));
                icons.Add("pd", Gdk.Pixbuf.LoadFromResource("cc-public-domain.png"));
                icons.Add("sampling", Gdk.Pixbuf.LoadFromResource("cc-sampling.png"));
                icons.Add("sampling+", Gdk.Pixbuf.LoadFromResource("cc-sampling-plus.png"));
                icons.Add("sa", Gdk.Pixbuf.LoadFromResource("cc-share-alike.png"));
                icons.Add("standard", Gdk.Pixbuf.LoadFromResource("cc-standard.png"));
		    }
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
			
			DrawLicense(window, widget, cell_area, state, flags);
		}
		
		public override void GetSize(Widget widget, ref Gdk.Rectangle cell_area, 
			out int x_offset, out int y_offset, out int width, out int height)
		{
		    height = icons["by"].Height + 2;
		    width = (icons["by"].Width * MAX_ATTRIBUTES) + 4;
		    x_offset = 0;
		    y_offset = 0;
		}
	
		private void DrawLicense(Gdk.Window canvas, Gtk.Widget widget,
			Gdk.Rectangle area, StateType state, CellRendererState flags)
		{
		    if(Track.License == null) {
		        return;
		    }
		    
		    split_result.Initialize();
            split_result = (Track.License).Split(splitter, MAX_ATTRIBUTES);
		    
		    for(int i = 0; i < split_result.Length; i++) {
				canvas.DrawPixbuf(widget.Style.TextGC(state), icons[split_result[i]], 0, 0,
					area.X + (i * icons["by"].Width) + 2, area.Y + 1, 
					icons["by"].Width, icons["by"].Height,
					RgbDither.None, 0, 0);
			}
		}
	}
}
