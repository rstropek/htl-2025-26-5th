const fs = require('fs')

const cfg = {
  apiBaseUrl: process.env.services__webapi__https__0 || process.env.services__webapi__http__0,
}

fs.writeFileSync(
  'src/environments/environment.development.ts',
  `export const environment = { apiBaseUrl: '${cfg.apiBaseUrl}' }\n`
)