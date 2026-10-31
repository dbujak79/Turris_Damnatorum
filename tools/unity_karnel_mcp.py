"""Świeża sesja lokalnego KarnelLabs Unity MCP przez stdio, bez uruchamiania drugiego Unity.

Użycie: python tools/unity_karnel_mcp.py --server <server/dist/cli.js> --request <JSON> --result <JSON>
Korzysta wyłącznie z już zainstalowanego serwera Node i SDK mcp. Nie instaluje pakietów ani nie czyta kluczy.
"""
import argparse
import asyncio
import json
import shutil
from datetime import timedelta
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client


def payload(result):
    if result.structuredContent is not None:
        return result.structuredContent
    texts=[entry.text for entry in result.content if hasattr(entry,'text')]
    if len(texts)==1:
        try:return json.loads(texts[0])
        except ValueError:return texts[0]
    return texts


async def main():
    parser=argparse.ArgumentParser(__doc__)
    parser.add_argument('--server',type=Path,required=True)
    parser.add_argument('--port',type=int,default=8099)
    parser.add_argument('--request',type=Path,required=True)
    parser.add_argument('--result',type=Path,required=True)
    parser.add_argument('--expect-project',type=Path,default=Path(__file__).resolve().parents[1])
    args=parser.parse_args()
    if not args.server.is_file():raise FileNotFoundError(args.server)
    calls=json.loads(args.request.read_text(encoding='utf-8-sig'))
    params=StdioServerParameters(command=shutil.which('node'),args=[str(args.server),f'--port={args.port}','--profile=core'],cwd=str(Path(__file__).resolve().parents[1]))
    outputs=[]
    async with stdio_client(params) as (read,write):
        async with ClientSession(read,write,read_timeout_seconds=timedelta(seconds=50)) as session:
            await session.initialize()
            for attempt in range(8):
                status=payload(await session.call_tool('unity_editor_connectionStatus',{}))
                if isinstance(status,dict) and status.get('connected'):break
                await asyncio.sleep(1)
            else:raise RuntimeError(f'Unity MCP bez połączenia: {status}')
            actual=str(status.get('dataPath','')).replace('\\','/').rstrip('/').casefold()
            expected=str(args.expect_project.resolve()/'Assets').replace('\\','/').rstrip('/').casefold()
            if actual!=expected:raise RuntimeError(f'Inny projekt Unity: {actual}; oczekiwano {expected}')
            for call in calls:
                response=await session.call_tool(call['name'],call.get('arguments',{}))
                data=payload(response)
                item={'name':call['name'],'isError':response.isError,'result':data}
                outputs.append(item)
                display=item
                if call['name']=='unity_script_compileCheck' and isinstance(data,dict):
                    display={'name':call['name'],'result':{key:data.get(key) for key in ['hasErrors','isCompiling','assemblyCount','errors']}}
                print(json.dumps(display,ensure_ascii=False),flush=True)
                args.result.parent.mkdir(parents=True,exist_ok=True)
                args.result.write_text(json.dumps(outputs,ensure_ascii=False,indent=2),encoding='utf-8')
                if response.isError or (isinstance(data,dict) and data.get('success') is False):raise RuntimeError(str(data))


if __name__=='__main__':
    import sys
    sys.stdout.reconfigure(encoding='utf-8',errors='replace')
    asyncio.run(main())
