# PDF Konvertering - Manual Guide

## ✅ Din HTML er klar!

Filen `EKSAMENSSPØRGSMÅL.html` er genereret og klar til at blive konverteret til PDF.

## 📄 Metode 1: Browser Print til PDF (Anbefalet)

1. **Åbn HTML-filen:**
   ```powershell
   Start-Process EKSAMENSSPØRGSMÅL.html
   ```
   (eller double-click på filen i File Explorer)

2. **I browseren, klik Ctrl+P (eller menu → Print)**

3. **I Print dialog:**
   - Destination: "Print to PDF" (eller "Gem som PDF")
   - Papirformat: A4
   - Marginer: Normal
   - Klik: "Gem" eller "Print"

4. **Gem som:** `EKSAMENSSPØRGSMÅL.pdf`

## 📌 Metode 2: Command Line (hvis du har system-dependencies)

```powershell
# Option A: wkhtmltopdf
wkhtmltopdf EKSAMENSSPØRGSMÅL.html EKSAMENSSPØRGSMÅL.pdf

# Option B: pandoc
pandoc EKSAMENSSPØRGSMÅL.md -o EKSAMENSSPØRGSMÅL.pdf --pdf-engine=xelatex
```

## 🎨 Hvad er i HTML-filen?

✅ Professionel styling med blå tema  
✅ Korrekt formatering af kode, tabeller, lister  
✅ Print-venlig layout  
✅ Dansk sprog med UTF-8 encoding  

## 📋 Indholdsfortegnelse

- Slide 1: De store udfordringer (microservices vs monolith)
- Slide 2: JWT + mTLS autentifikation
- Slide 3: Secret Management
- Slide 4: Kryptering i transit
- Slide 5: Logging & audit trail
- Slide 6-10: RabbitMQ sikkerhed (3 lag)
- Slide 11: Defense-in-depth model
- Slide 12: Key takeaways
- Slide 13: Vigtige filer

**Total: ~15 slides = 10-15 min presentation** ✅
