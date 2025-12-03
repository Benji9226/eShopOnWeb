#!/usr/bin/env python3
"""
Konvertér HTML til PDF med WeasyPrint
"""

import subprocess
import sys

# Installér WeasyPrint hvis ikke allerede installeret
try:
    import weasyprint
except ImportError:
    print("📦 Installerer WeasyPrint...")
    subprocess.check_call([sys.executable, "-m", "pip", "install", "weasyprint", "-q"])
    import weasyprint

from pathlib import Path
from weasyprint import HTML, CSS

# Filer
html_file = Path("EKSAMENSSPØRGSMÅL.html")
pdf_file = Path("EKSAMENSSPØRGSMÅL.pdf")

# Konvertér
print(f"🔄 Konverterer {html_file} til PDF...")
try:
    HTML(str(html_file)).write_pdf(str(pdf_file))
    print(f"✅ PDF genereret: {pdf_file}")
    print(f"📄 Størrelse: {pdf_file.stat().st_size / 1024:.1f} KB")
except Exception as e:
    print(f"❌ Fejl: {e}")
    print(f"💡 Prøv at åbne {html_file} i en browser og brug 'Print til PDF'")
