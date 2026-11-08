document.addEventListener("DOMContentLoaded", () => {
    const menuButton = document.getElementById("mobileMenu");
    const nav = document.getElementById("siteNav");
    const customValidationFormSelector = "#registerForm, #loginForm, #incidentForm, #analyticsFilter";
    const sharedLoadingExclusionSelector = `[data-managed-loading], ${customValidationFormSelector}`;
    function closeMenu(returnFocus) {
        if (!menuButton || !nav) return;
        nav.classList.remove("open");
        menuButton.setAttribute("aria-expanded", "false");
        menuButton.setAttribute("aria-label", "Open navigation menu");
        if (returnFocus) menuButton.focus();
    }

    menuButton?.addEventListener("click", () => {
        const expanded = menuButton.getAttribute("aria-expanded") === "true";
        menuButton.setAttribute("aria-expanded", String(!expanded));
        menuButton.setAttribute("aria-label", expanded ? "Open navigation menu" : "Close navigation menu");
        nav?.classList.toggle("open", !expanded);
        if (!expanded)
            nav?.querySelector(".role-nav:not([hidden]) a, .role-nav:not([hidden]) button")?.focus();
    });

    nav?.addEventListener("keydown", (event) => {
        const items = [...nav.querySelectorAll(".role-nav:not([hidden]) a, .role-nav:not([hidden]) button")];
        const index = items.indexOf(document.activeElement);
        if (event.key === "Escape") closeMenu(true);
        if (index < 0) return;
        if (event.key === "ArrowDown" || event.key === "ArrowRight") {
            event.preventDefault();
            items[(index + 1) % items.length].focus();
        } else if (event.key === "ArrowUp" || event.key === "ArrowLeft") {
            event.preventDefault();
            items[(index - 1 + items.length) % items.length].focus();
        } else if (event.key === "Home") {
            event.preventDefault();
            items[0].focus();
        } else if (event.key === "End") {
            event.preventDefault();
            items[items.length - 1].focus();
        }
    });

    function updateActiveNavigation() {
        nav?.querySelectorAll('a[aria-current="page"]').forEach((link) =>
            link.removeAttribute("aria-current"),
        );
        nav?.querySelectorAll(".role-nav:not([hidden]) a").forEach((link) => {
            const rawHref = link.getAttribute("href");
            if (!rawHref || rawHref === "#" || rawHref.startsWith("#")) return;
            const target = new URL(link.href, window.location.href);
            if (
                target.pathname.toLowerCase() === window.location.pathname.toLowerCase() &&
                target.hash === window.location.hash
            ) {
                link.setAttribute("aria-current", "page");
            }
        });
    }

    updateActiveNavigation();

    document.addEventListener(
        "invalid",
        (event) => {
            const field = event.target;
            if (!(field instanceof HTMLElement) || !("checkValidity" in field)) return;
            if (field.form?.matches(customValidationFormSelector)) return;
            const wrapper = field.closest("label") || field.parentElement;
            if (!wrapper) return;
            let message = wrapper.querySelector(":scope > .inline-error");
            if (!message) {
                message = document.createElement("span");
                message.className = "inline-error";
                message.setAttribute("role", "alert");
                field.insertAdjacentElement("afterend", message);
            }
            const icon = document.createElement("span");
            icon.className = "error-icon";
            icon.setAttribute("aria-hidden", "true");
            icon.textContent = "!";
            message.replaceChildren(
                icon,
                document.createTextNode(` ${field.validationMessage || "Please check this field."}`),
            );
            field.setAttribute("aria-invalid", "true");
            if (message.id) field.setAttribute("aria-describedby", message.id);
        },
        true,
    );

    document.addEventListener("input", (event) => {
        const field = event.target;
        if (!(field instanceof HTMLElement) || !("checkValidity" in field)) return;
        if (field.form?.matches(customValidationFormSelector)) return;
        if (field.checkValidity()) {
            field.removeAttribute("aria-invalid");
            const describedBy = (field.getAttribute("aria-describedby") || "").split(/\s+/).filter(Boolean);
            let error = describedBy
                .map((id) => document.getElementById(id))
                .find((node) => node?.classList.contains("inline-error"));
            if (!error && field.nextElementSibling?.classList.contains("inline-error"))
                error = field.nextElementSibling;
            if (!error && field.parentElement?.nextElementSibling?.classList.contains("inline-error"))
                error = field.parentElement.nextElementSibling;
            if (error?.id) {
                error.textContent = "";
                error.hidden = true;
            } else {
                error?.remove();
            }
        }
    });

    document.addEventListener(
        "submit",
        (event) => {
            if (
                !(event.target instanceof HTMLFormElement) ||
                event.target.matches(
                    sharedLoadingExclusionSelector,
                ) ||
                !event.target.checkValidity()
            )
                return;
            const button = event.target.querySelector('button[type="submit"], button:not([type])');
            if (!button || button.disabled) return;
            button.dataset.originalText = button.textContent;
            button.textContent = button.dataset.loadingText || "Please wait…";
            button.disabled = true;
            button.setAttribute("aria-busy", "true");
        },
        true,
    );
});

function setInlineError(field, error, message) {
    if (!field || !error) return;
    error.replaceChildren();
    if (message) {
        const icon = document.createElement("span");
        icon.className = "error-icon";
        icon.setAttribute("aria-hidden", "true");
        icon.textContent = "!";
        error.append(icon, document.createTextNode(` ${message}`));
        error.hidden = false;
        field.setAttribute("aria-invalid", "true");
        if (error.id) {
            const ids = (field.getAttribute("aria-describedby") || "").split(/\s+/).filter(Boolean);
            if (!ids.includes(error.id)) ids.push(error.id);
            field.setAttribute("aria-describedby", ids.join(" "));
        }
    } else {
        error.hidden = true;
        field.removeAttribute("aria-invalid");
    }
}
