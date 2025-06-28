const itemsContainer = document.getElementById("items");

function formatDateDifference(newDate) {
  let dateNow = new Date();
  let dateAddedOffer = new Date(newDate);

  let differenceMS = dateNow - dateAddedOffer;
  let differenceMIN = Math.floor(differenceMS / (1000 * 60));
  let differenceHOUR = Math.floor(differenceMIN / 60);

  return differenceHOUR > 0
    ? `Dodano: ${differenceHOUR} g ${differenceMIN} min temu`
    : `Dodano: ${differenceMIN} min temu`;
}

function fetchOffers() {
  fetch("http://localhost:5261/offers") // replace with your proxy url
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

        itemDiv.innerHTML +=
          "<p>" + formatDateDifference(offer.dateAdded) + "</p>";

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
}, 1000);
