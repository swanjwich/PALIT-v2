// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showToast(message, type) {
    const toast = document.createElement("div");

    const bgColor = {
        success: "bg-green-400 text-white",
        error: "bg-red-400 text-white",
        warning: "bg-yellow-400 text-black",
        info: "bg-blue-400 text-white"
    }[type] || "bg-blue-400";

    const iconClass = {
        success: "text-green-500 bg-green-100",
        error: "text-red-500 bg-red-100",
        warning: "text-orange-500 bg-orange-100",
        info: "text-blue-500 bg-blue-100"
    }[type] || "text-blue-500 bg-blue-100";

    const svgPath = {
        success: "M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5Zm3.707 8.207-4 4a1 1 0 0 1-1.414 0l-2-2a1 1 0 0 1 1.414-1.414L9 10.586l3.293-3.293a1 1 0 0 1 1.414 1.414Z",
        error: "M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5Zm3.707 11.793a1 1 0 1 1-1.414 1.414L10 11.414l-2.293 2.293a1 1 0 0 1-1.414-1.414L8.586 10 6.293 7.707a1 1 0 0 1 1.414-1.414L10 8.586l2.293-2.293a1 1 0 0 1 1.414 1.414L11.414 10l2.293 2.293Z",
        warning: "M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5ZM10 14a1 1 0 1 1 0-2 1 1 0 0 1 0 2Zm1-4a1 1 0 0 1-2 0V6a1 1 0 0 1 2 0v4Z",
        info: "M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5ZM10 15a1 1 0 1 1 0-2 1 1 0 0 1 0 2Zm1-4a1 1 0 0 1-2 0V6a1 1 0 0 1 2 0v5Z"
    }[type] || "";

    toast.className = `fixed top-34 right-4 ${bgColor} shadow-lg rounded-lg p-4 z-50 flex items-center max-w-xs animate-toast-in transition-opacity duration-500`;
    toast.innerHTML = `
            <div class="inline-flex items-center justify-center shrink-0 w-8 h-8 ${iconClass} rounded-lg">
                <svg class="w-5 h-5" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 20 20">
                    <path d="${svgPath}" />
                </svg>
            </div>
            <div class="ms-3 text-sm font-normal">${message}</div>
        `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.add("opacity-0");
        setTimeout(() => toast.remove(), 500);
    }, 4000);
}

function toggleTheme() {
    const html = document.documentElement;
    const toggleIcon = document.getElementById('toggleIcon');
    const isDark = html.classList.toggle('dark');

    if (isDark) {
        toggleIcon.innerHTML = '<path d="M8 5H16C19.866 5 23 8.13401 23 12C23 15.866 19.866 19 16 19H8C4.13401 19 1 15.866 1 12C1 8.13401 4.13401 5 8 5ZM16 15C17.6569 15 19 13.6569 19 12C19 10.3431 17.6569 9 16 9C14.3431 9 13 10.3431 13 12C13 13.6569 14.3431 15 16 15Z"/>';
    } else {
        toggleIcon.innerHTML = '<path d="M8 7C5.23858 7 3 9.23858 3 12C3 14.7614 5.23858 17 8 17H16C18.7614 17 21 14.7614 21 12C21 9.23858 18.7614 7 16 7H8ZM8 5H16C19.866 5 23 8.13401 23 12C23 15.866 19.866 19 16 19H8C4.13401 19 1 15.866 1 12C1 8.13401 4.13401 5 8 5ZM8 15C6.34315 15 5 13.6569 5 12C5 10.3431 6.34315 9 8 9C9.65685 9 11 10.3431 11 12C11 13.6569 9.65685 15 8 15Z"/>';
    }

    localStorage.setItem('theme', isDark ? 'dark' : 'light');
}

document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme');
    const toggleIcon = document.getElementById('toggleIcon');

    if (savedTheme === 'dark') {
        document.documentElement.classList.add('dark');
        toggleIcon.innerHTML = '<path d="M8 5H16C19.866 5 23 8.13401 23 12C23 15.866 19.866 19 16 19H8C4.13401 19 1 15.866 1 12C1 8.13401 4.13401 5 8 5ZM16 15C17.6569 15 19 13.6569 19 12C19 10.3431 17.6569 9 16 9C14.3431 9 13 10.3431 13 12C13 13.6569 14.3431 15 16 15Z"/>';
    }
});

