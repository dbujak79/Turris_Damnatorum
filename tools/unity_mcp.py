"""Klient lokalnego Unity MCP: diagnostyka i testy w już otwartym edytorze.

Wymaga pakietu Python mcp i serwera wskazanego w konfiguracji Codex.
--list / --schemas <nazwy> / --resources / --request <plik JSON z listą wywołań>.
Nie uruchamia ani nie zamyka Unity.
"""
import argparse
import asyncio
import json
import sys
from datetime import timedelta
from pathlib import Path

from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client


async def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--url', default='http://127.0.0.1:8080/mcp')
    parser.add_argument('--list', action='store_true')
    parser.add_argument('--schemas', nargs='+')
    parser.add_argument('--resources', action='store_true')
    parser.add_argument('--request', type=Path)
    parser.add_argument('--instance')
    parser.add_argument('--wait', action='store_true', help='Poczekaj na wynik uruchomionego testu.')
    parser.add_argument('--result', type=Path, help='Zapisz końcowy wynik testu w JSON.')
    args = parser.parse_args()
    async with streamablehttp_client(args.url, timeout=15, sse_read_timeout=60) as (read, write, _):
        async with ClientSession(read, write, read_timeout_seconds=timedelta(seconds=55)) as session:
            await session.initialize()
            if args.instance:
                print((await session.call_tool('set_active_instance', {'instance': args.instance})).model_dump_json(), flush=True)
            if args.list or args.schemas:
                result = await session.list_tools()
                for tool in result.tools:
                    if args.list:
                        print(tool.name + ': ' + (tool.description or '').split('\n')[0])
                    elif tool.name in args.schemas:
                        print(json.dumps({'name': tool.name, 'description': tool.description, 'inputSchema': tool.inputSchema}, ensure_ascii=False, indent=2))
            elif args.resources:
                for resource in (await session.list_resources()).resources:
                    print(str(resource.uri) + ': ' + resource.name)
            elif args.request:
                calls = json.loads(args.request.read_text(encoding='utf-8-sig'))
                for call in calls:
                    if 'resource' in call:
                        result = await session.read_resource(call['resource'])
                    else:
                        result = await session.call_tool(call['name'], call.get('arguments', {}))
                    if hasattr(result, 'contents'):
                        for content in result.contents:
                            print(content.text, flush=True)
                    else:
                        payload = result.structuredContent
                        if payload is None:
                            payload = [c.text for c in result.content if hasattr(c, 'text')]
                        print(json.dumps(payload, ensure_ascii=False, indent=2), flush=True)
                        if args.wait and call.get('name') == 'run_tests':
                            response = payload.get('result', payload)
                            job = (response.get('data') or {}).get('job_id')
                            while job:
                                status = await session.call_tool('get_test_job', {'job_id': job, 'wait_timeout': 30, 'include_details': True, 'include_failed_tests': True})
                                state = status.structuredContent
                                if state is None:
                                    state = json.loads(next(c.text for c in status.content if hasattr(c, 'text')))
                                data = state.get('result', state).get('data') or {}
                                progress = data.get('progress') or {}
                                print(json.dumps({'job_id': job, 'status': data.get('status'), 'completed': progress.get('completed'), 'total': progress.get('total')}, ensure_ascii=False), flush=True)
                                if data.get('status') != 'running':
                                    if args.result:
                                        args.result.parent.mkdir(parents=True, exist_ok=True)
                                        args.result.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
                                    print(json.dumps(data.get('result') or {'error': data.get('error'), 'failures': progress.get('failures_so_far')}, ensure_ascii=False, indent=2), flush=True)
                                    break


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    asyncio.run(main())
