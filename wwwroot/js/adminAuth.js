
document.addEventListener("DOMContentLoaded", function () {
    const showRegister = document.getElementById("showRegister");
    const showLogin = document.getElementById("showLogin");
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    if (showRegister && showLogin && loginForm && registerForm) {
        showRegister.addEventListener("click", function () {
            loginForm.style.display = "none";
            registerForm.style.display = "block";
        });

        showLogin.addEventListener("click", function () {
            registerForm.style.display = "none";
            loginForm.style.display = "block";
        });
    }
});
