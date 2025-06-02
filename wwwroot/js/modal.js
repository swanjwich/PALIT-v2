document.querySelectorAll("[data-modal-target]").forEach(openBtn => {
    const modalId = openBtn.getAttribute("data-modal-target");
    const modal = document.getElementById(modalId);
    const modalContent = modal.querySelector(".modal-content");

    openBtn.addEventListener("click", () => {
        modal.classList.remove("opacity-0", "pointer-events-none");
        modal.classList.add("opacity-100");

        setTimeout(() => {
            modalContent.classList.remove("opacity-0", "scale-95");
            modalContent.classList.add("opacity-100", "scale-100");
        }, 50);
    });

    // Close buttons inside this modal
    modal.querySelectorAll("[data-modal-close]").forEach(closeBtn => {
        closeBtn.addEventListener("click", () => {
            modalContent.classList.remove("opacity-100", "scale-100");
            modalContent.classList.add("opacity-0", "scale-95");

            setTimeout(() => {
                modal.classList.remove("opacity-100");
                modal.classList.add("opacity-0", "pointer-events-none");
            }, 300);
        });
    });

    // Close when clicking outside modal content
    modal.addEventListener("click", (e) => {
        if (e.target === modal) {
            modalContent.classList.remove("opacity-100", "scale-100");
            modalContent.classList.add("opacity-0", "scale-95");

            setTimeout(() => {
                modal.classList.remove("opacity-100");
                modal.classList.add("opacity-0", "pointer-events-none");
            }, 300);
        }
    });
});
