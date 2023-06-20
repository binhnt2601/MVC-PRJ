namespace App.Models
{
    public class Summernote
    {
        public Summernote(string id, bool isLoaded = true)
        {
            Id = id;
            IsLoaded = isLoaded;
        }

        public string Id { get; set; }
        public bool IsLoaded { get; set; }

        public int height { get; set; } = 120;
        public string toolbar {get; set; } = @"[
                ['style', ['bold', 'italic', 'underline', 'clear']],
                ['font', ['strikethrough', 'superscript', 'subscript']],
                ['fontsize', ['fontsize']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['height', ['height']],
                ['view', ['fullscreen', 'codeview', 'help']]
                ]";

    }
}