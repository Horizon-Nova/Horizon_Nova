/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "../../Views/**/*.cshtml",
    "../../../../wwwroot/ww/js/**/*.js"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Lato", "sans-serif"],
        serif: ["Bodoni Moda", "serif"]
      }
    }
  }
};
