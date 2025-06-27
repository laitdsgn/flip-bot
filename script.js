const itemsContainer = document.getElementById("items");

function fetchOffers() {
  fetch("http://localhost:5261/offers")
    .then((response) => response.json())
    .then((data) => {
      console.log(data);
      itemsContainer.innerHTML = ""; // Clear existing items
      data.forEach((offer, index) => {
        const itemDiv = document.createElement("div");
        itemDiv.className = "item";
        itemDiv.innerHTML = `
          <h3>${offer.title}</h3>
          <p>${offer.price}</p>
          <p>${offer.date}</p>
        `;

        if (offer.img !== null) {
          itemDiv.innerHTML += `<div class='photo-container'><img src="${offer.img}" alt="${offer.title}" class="item-image" width="60%" /></div>`;
        }
        itemDiv.innerHTML += `<a href="${offer.link}" target="_blank"><button>Oferta</button></a>`;

        itemsContainer.appendChild(itemDiv);
      });
    })
    .catch((error) => {
      console.error("Error fetching offers:", error);
    });
}

fetchOffers();

setInterval(() => {
  fetchOffers();
}, 1000 * 60);
