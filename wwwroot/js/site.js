// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
  // Get all <li> elements within the navigation menu
  const navItems = document.querySelectorAll(".nav_active");

  // Add click event listener to each <li> element
  navItems.forEach(function (item) {
    item.addEventListener("click", function () {
      // Remove 'active' class from all <li> elements
      navItems.forEach(function (navItem) {
        navItem.classList.remove("active");
      });

      // Add 'active' class to the clicked <li> element
      this.classList.add("active");
    });
  });
});
document.addEventListener("DOMContentLoaded", function () {
  const decreaseBtn = document.getElementById("decrease-btn");
  const increaseBtn = document.getElementById("increase-btn");
  const numberInput = document.getElementById("number-input");

  decreaseBtn.addEventListener("click", function () {
    decreaseNumber();
  });

  increaseBtn.addEventListener("click", function () {
    increaseNumber();
  });

  function decreaseNumber() {
    let currentValue = parseInt(numberInput.value);
    if (currentValue > 0) {
      numberInput.value = currentValue - 1;
    }
  }

  function increaseNumber() {
    let currentValue = parseInt(numberInput.value);
    numberInput.value = currentValue + 1;
  }
});
// password
const togglePassword = document.querySelector("#togglePassword");
const password = document.querySelector("#password");
const eyeIcon = document.querySelector(".eye");

togglePassword.addEventListener("click", () => {
  // Toggle the type attribute of password field
  const type =
    password.getAttribute("type") === "password" ? "text" : "password";
  password.setAttribute("type", type);

  // Toggle the eye and eye-off icons
  if (type === "password") {
    eyeIcon.setAttribute("name", "eye-outline");
  } else {
    eyeIcon.setAttribute("name", "eye-off-outline");
  }
});
const toggleConfirmPassword = document.querySelector("#toggleConfirmPassword");
const confirmPassword = document.querySelector("#confirmPassword");
const confirmEyeIcon = toggleConfirmPassword.querySelector("ion-icon");

toggleConfirmPassword.addEventListener("click", () => {
  // Toggle the type attribute of confirm password field
  const type =
    confirmPassword.getAttribute("type") === "password" ? "text" : "password";
  confirmPassword.setAttribute("type", type);

  // Toggle the eye and eye-off icons
  if (type === "password") {
    confirmEyeIcon.setAttribute("name", "eye-outline");
  } else {
    confirmEyeIcon.setAttribute("name", "eye-off-outline");
  }
});
