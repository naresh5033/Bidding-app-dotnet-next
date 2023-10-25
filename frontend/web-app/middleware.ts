export { default } from "next-auth/middleware"

export const config = {
    matcher: [
        '/session' //wanna protect the session route
    ],
    pages: {
        signIn: '/api/auth/signin'
    }
}