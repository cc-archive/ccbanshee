/***************************************************************************
 *  LicenseRenderer.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
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
 
using Gtk;
using Gdk;

using Banshee.Base;

namespace Banshee.Gui
{
	public class LicenseRenderer : CellRenderer
	{
	    public TrackInfo Track;
        private static Gdk.Pixbuf[] icon_lookup;
		
		public LicenseRenderer()
		{
		    if(icon_lookup == null)
		        LoadIcons();
		}

		protected LicenseRenderer(System.IntPtr ptr) : base(ptr)
		{
		    if(icon_lookup == null)
		        LoadIcons();
		}
        
        private void LoadIcons() {
            System.Array values = System.Enum.GetValues(typeof(Licenses.Attributes));
            icon_lookup = new Gdk.Pixbuf[values.Length];
            
            foreach(Licenses.Attributes la in values) {
                if(la != Licenses.Attributes.none)
                    icon_lookup[(int)la] = Gdk.Pixbuf.LoadFromResource("cc-" + la.ToString() + ".png");
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
		    height = icon_lookup[(int)Licenses.Attributes.nd].Height + 2;
		    width = (icon_lookup[(int)Licenses.Attributes.nd].Width * 3) + 4;
		    x_offset = 0;
		    y_offset = 0;
		}
	
		private void DrawLicense(Gdk.Window canvas, Gtk.Widget widget,
			Gdk.Rectangle area, StateType state, CellRendererState flags)
		{
		    if(!ShowLicense(Track))
		        return;
    
			canvas.DrawPixbuf(widget.Style.TextGC(state), icon_lookup[(int)Track.License], 0, 0,
               area.X + 2, area.Y + 1, icon_lookup[(int)Track.License].Width,
               icon_lookup[(int)Track.License].Height, RgbDither.None, 0, 0);
		}
		
		public bool ShowLicense(TrackInfo track)
        {
            if(track.License == Licenses.Attributes.none) // No License
		        return false;
		    else if(track.LicenseVerifyStatus == LicenseVerifyStatus.Invalid) // Invalid License
		        return false;
		    else
		        return true;
        }
	}
}
