#!/usr/bin/env python3
"""
scripts/generate_and_package.py

Simple script to generate a short music file using Suno (or fail gracefully),
save to an output folder, and create a zip package suitable for ZipDeploy.

This is a template: you must add SUNO_API_KEY to GitHub secrets, and optionally
SUNO_ENDPOINT if using a custom Suno deployment.
"""
import os
import sys
import argparse
import json
import shutil
from pathlib import Path

try:
    import requests
except Exception:
    print('Please install dependencies (pip install -r scripts/requirements.txt)')
    raise


def generate_with_suno(title: str, out_path: Path):
    api_key = os.environ.get('SUNO_API_KEY')
    if not api_key:
        raise RuntimeError('Suno API key missing. Set SUNO_API_KEY in environment or GitHub secrets.')
    # Minimal example using a hypothetical Suno REST API endpoint.
    # Replace with provider-specific API calls.
    endpoint = os.environ.get('SUNO_ENDPOINT', 'https://api.suno.ai/v1/generate')
    payload = {
        'title': title,
        'duration': 20,
        'format': 'mp3',
        'voice': 'female_singer',
        'style': 'ambient pop, intimate vocal, piano and strings, 90 BPM'
    }
    headers = {'Authorization': f'Bearer {api_key}', 'Content-Type': 'application/json'}
    print('Requesting Suno generation...')
    r = requests.post(endpoint, headers=headers, json=payload, stream=True)
    if r.status_code != 200:
        raise RuntimeError(f'Suno API failed: {r.status_code} {r.text}')
    out_file = out_path / (title + '.mp3')
    with open(out_file, 'wb') as f:
        for chunk in r.iter_content(1024 * 32):
            if chunk:
                f.write(chunk)
    print('Wrote', out_file)
    return out_file


def package_deploy(out_dir: Path, zip_path: Path):
    # Ensure wwwroot structure
    www = out_dir / 'wwwroot'
    music = www / 'music'
    music.mkdir(parents=True, exist_ok=True)
    # Move any mp3s into wwwroot/music
    for mp3 in out_dir.glob('*.mp3'):
        shutil.move(str(mp3), str(music / mp3.name))
    # Create a music_data.json stub
    files = []
    i = 1
    for f in sorted(music.iterdir()):
        if f.suffix.lower() == '.mp3':
            files.append({
                'Id': i,
                'FileName': f.name,
                'OriginalFileName': f.name,
                'Category': 'AI',
                'CreatedAt': '',
                'FileSize': f.stat().st_size,
                'Description': ''
            })
            i += 1
    with open(www.parent / 'music_data.json', 'w', encoding='utf-8') as fh:
        json.dump(files, fh, ensure_ascii=False, indent=2)
    # Zip the publish folder
    shutil.make_archive(str(zip_path.with_suffix('')), 'zip', root_dir=str(out_dir))
    print('Created', zip_path)


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('--out-dir', default='./publish', help='Output folder to place generated files')
    p.add_argument('--title', default='ai_song', help='Title / filename prefix')
    args = p.parse_args()

    out_path = Path(args.out_dir)
    out_path.mkdir(parents=True, exist_ok=True)

    try:
        mp3 = generate_with_suno(args.title, out_path)
    except Exception as e:
        print('Suno generation failed:', e)
        sys.exit(2)

    zip_path = Path('deploy_package.zip')
    package_deploy(out_path, zip_path)
