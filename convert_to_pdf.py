#!/usr/bin/env python3
"""
Konvertér Markdown til PDF med styling
"""

import markdown
from pathlib import Path

# Læs markdown fil
md_file = Path("EKSAMENSSPØRGSMÅL.md")
html_file = Path("EKSAMENSSPØRGSMÅL.html")
pdf_file = Path("EKSAMENSSPØRGSMÅL.pdf")

# Konvertér markdown til HTML
with open(md_file, 'r', encoding='utf-8') as f:
    md_content = f.read()

# Basic HTML template med styling
html_template = """<!DOCTYPE html>
<html lang="da">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sikkerhed i Microservices - eShopOnWeb</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background: white;
            padding: 20px;
            max-width: 900px;
            margin: 0 auto;
        }
        
        h1 {
            color: #1a5490;
            margin-top: 30px;
            margin-bottom: 10px;
            border-bottom: 3px solid #1a5490;
            padding-bottom: 10px;
            font-size: 2em;
        }
        
        h2 {
            color: #2e7db5;
            margin-top: 25px;
            margin-bottom: 15px;
            font-size: 1.6em;
        }
        
        h3 {
            color: #4a90e2;
            margin-top: 20px;
            margin-bottom: 10px;
            font-size: 1.3em;
        }
        
        h4 {
            color: #5a9fd4;
            margin-top: 15px;
            margin-bottom: 8px;
            font-size: 1.1em;
        }
        
        p {
            margin-bottom: 12px;
            text-align: justify;
        }
        
        code {
            background-color: #f5f5f5;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
            color: #d63384;
        }
        
        pre {
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
            overflow-x: auto;
            border-left: 4px solid #2e7db5;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
        }
        
        pre code {
            background: none;
            color: #333;
            padding: 0;
        }
        
        ul, ol {
            margin-left: 20px;
            margin-bottom: 12px;
        }
        
        li {
            margin-bottom: 6px;
        }
        
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 15px 0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        th {
            background-color: #1a5490;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
        }
        
        td {
            border: 1px solid #ddd;
            padding: 10px;
        }
        
        tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        
        tr:hover {
            background-color: #f0f0f0;
        }
        
        blockquote {
            border-left: 4px solid #2e7db5;
            margin: 15px 0;
            padding-left: 15px;
            color: #666;
            font-style: italic;
        }
        
        hr {
            border: none;
            border-top: 2px solid #ccc;
            margin: 30px 0;
            page-break-after: always;
        }
        
        .emoji {
            font-size: 1.2em;
        }
        
        @media print {
            body {
                padding: 0;
            }
            
            h1, h2 {
                page-break-after: avoid;
            }
            
            pre {
                page-break-inside: avoid;
            }
            
            table {
                page-break-inside: avoid;
            }
        }
    </style>
</head>
<body>
{content}
</body>
</html>
"""

# Konvertér markdown til HTML
html_content = markdown.markdown(md_content, extensions=['tables', 'fenced_code'])

# Wrap i template - brug replace i stedet for format for at undgå {}
final_html = html_template.replace("{content}", html_content)

# Gem HTML
with open(html_file, 'w', encoding='utf-8') as f:
    f.write(final_html)

print(f"✅ HTML genereret: {html_file}")
print(f"\n📌 For at konvertere til PDF, åbn HTML-filen i en browser og brug 'Print til PDF'")
print(f"\nAlternativ - brug denne kommando hvis du har wkhtmltopdf:")
print(f"   wkhtmltopdf {html_file} {pdf_file}")
