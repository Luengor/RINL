/** @type {import('@hey-api/openapi-ts').UserConfig} */
module.exports = {
    input: 'openapi.json',
    output: 'src/client',
    plugins: ['@hey-api/client-fetch'],
};