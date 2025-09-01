import uvicorn
from app.api.catalogs import app

if __name__ == "__main__":
    uvicorn.run(
        "app.api.catalogs:app",
        host="0.0.0.0",
        port=8000,
        reload=True
    )