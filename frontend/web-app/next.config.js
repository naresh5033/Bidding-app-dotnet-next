/** @type {import('next').NextConfig} */
const nextConfig = {
    experimental: {
        serverActions: true
    },
    images: {
        domains: [
            'cdn.pixabay.com'
        ]
    },
    output: 'standalone' // so we can use this for the dockerize our app
}

module.exports = nextConfig
