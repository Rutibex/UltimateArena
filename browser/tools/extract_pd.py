"""Extract original PD weapon data; keep origin and arena tuning separate."""
import re,json,pathlib
root=pathlib.Path(__file__).resolve().parents[1]
s=(root/'vendor/perfect-dark/invitems.c').read_text()
names=['falcon2_singleshot','cyclone_rapidfire','shotgun_single','sniperrifle_singleshot','rockerlauncher_shoot','devastator_shoot']
result=[]
for name in names:
 b=re.search(r'struct weaponfunc_\w+ invfunc_'+name+r' = \{(.*?)\n\};',s,re.S).group(1)
 fields={}
 for line in b.splitlines():
  m=re.match(r'\s*([-\d.]+),\s*// (\w+)',line)
  if m: fields[m[2]]=float(m[1])
 result.append({'symbol':'invfunc_'+name,**fields})
(root/'shared/pd-data.js').write_text('// Generated from vendor/perfect-dark/invitems.c by tools/extract_pd.py.\nexport const PD_DATA = '+json.dumps(result,indent=2)+';\n')
print('Extracted',len(result),'Perfect Dark firing definitions')
