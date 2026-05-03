import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: process.env['SWAGGER_URL'] ?? 'http://localhost:5000/swagger/v1/swagger.json',
  output: {
    path: 'src/generated',
    format: 'prettier',
  },
  client: '@hey-api/client-fetch',
});
