// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.musicxml
{
    public partial class notations
    {
        public void AddTiedStart()
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new tied { type = startstopcontinue.start });
        }

        public void AddTiedStop()
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new tied { type = startstopcontinue.stop });
        }
    }
}
