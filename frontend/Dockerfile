# Faza 1: Build aplikacije
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# Ovo pravi 'dist' folder sa optimizovanim fajlovima
RUN npm run build

# Faza 2: Nginx server koji služi statične fajlove
FROM nginx:alpine
# Kopiramo build-ovane fajlove u Nginx folder
COPY --from=build /app/dist /usr/share/nginx/html
# Kopiramo našu nginx konfiguraciju
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 5173
CMD ["nginx", "-g", "daemon off;"]