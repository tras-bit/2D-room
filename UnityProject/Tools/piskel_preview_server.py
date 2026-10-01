#!/usr/bin/env python3
"""Serve the local Piskel editor with the selected project loaded on startup."""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[2]
EDITOR = ROOT / ".tools" / "Piskel" / "dest" / "prod"
PROJECT = ROOT / "UnityProject" / "ArtSource" / "individual_models.piskel"
PROJECT_DATA = json.loads(PROJECT.read_text(encoding="utf-8"))["piskel"]

class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(EDITOR), **kwargs)

    def do_GET(self):
        if self.path in ("/", "/index.html"):
            html = (EDITOR / "index.html").read_text(encoding="utf-8")
            payload = json.dumps(PROJECT_DATA, ensure_ascii=True, separators=(",", ":"))
            loader = f"""<script>
window.piskelReadyCallbacks = window.piskelReadyCallbacks || [];
window.piskelReadyCallbacks.push(function () {{
  window._externalPiskel = {payload};
  if (window.$ && window.Events) window.$.publish(window.Events.EXTERNAL_PISKEL_READY);
}});
</script>"""
            html = html.replace("</body>", loader + "</body>")
            body = html.encode("utf-8")
            self.send_response(200)
            self.send_header("Content-Type", "text/html; charset=utf-8")
            self.send_header("Content-Length", str(len(body)))
            self.send_header("Cache-Control", "no-store")
            self.end_headers()
            self.wfile.write(body)
            return
        return super().do_GET()

if __name__ == "__main__":
    if not EDITOR.is_dir() or not PROJECT.is_file():
        raise SystemExit("Piskel production build or individual_models.piskel is missing")
    server = ThreadingHTTPServer(("0.0.0.0", 8080), Handler)
    print("Piskel editor ready on port 8080 with individual pixel models loaded", flush=True)
    server.serve_forever()
