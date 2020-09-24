/*
//todo 32비트가 아닌 24비트로 저장하는 방법 찾기
public byte[] Merge(List<byte[]> webtoondatas)
{
    ImageConverter imageConverter = new ImageConverter();
    List<Bitmap> images = new List<Bitmap>();
    for (int i = 0; i < webtoondatas.Count; i++)
    {
        images.Add(new Bitmap((Image)imageConverter.ConvertFrom(webtoondatas[i])));
    }
    int width = images.First().Width;
    int height = 0;
    for (int i = 0; i < images.Count; i++)
    {
        height += images[i].Height;
    }
    Bitmap bitmap2 = new Bitmap(width, height);
    bitmap2.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution); // <-- Set explicit resolution on bitmap2
                                                                                         // Always put Graphics objects in a 'using' block.
    using (Graphics g = Graphics.FromImage(bitmap2))
    {
        height = 0;
        for (int i = 0; i < images.Count; i++)
        {
            Bitmap image = images[i];
            image.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution); // <-- Set resolution equal to bitmap2
            g.DrawImage(image, 0, height);
            height += image.Height;
            images[i].Dispose();//램? dispose?
        }
    }
    return (byte[])imageConverter.ConvertTo(bitmap2, typeof(byte[]));
}
*/