const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.refreshTickets = functions.https.onCall(async (data, context) => {
  try {
    const userId = context.auth.uid;
    if (!userId) {
      throw new functions.https.HttpsError("unauthenticated", " thenticated.");
    }

    console.log("User ID:", userId);

    const userRef = admin.firestore().collection("users").doc(userId);
    const userDoc = await userRef.get();

    if (!userDoc.exists) {
      console.log("User document not found for ID:", userId);
      throw new functions.https.HttpsError("not-found", "User not found");
    }

    const userData = userDoc.data();
    console.log("User data retrieved:", userData);

    const currentTickets = userData.tickets || 0;
    const lastRefresh = userData.lastRefresh ? userData.lastRefresh.toDate() :
          new Date(0);
    console.log("Current Tickets:", currentTickets
        , "Last Refresh:", lastRefresh);

    const maxTickets = 5;
    const refreshInterval = 10 * 60 * 1000; // 10 minutes in milliseconds

    const now = new Date();
    const timeElapsed = now - lastRefresh;
    console.log("Time Elapsed since last refresh (ms):", timeElapsed);

    let updatedTickets = currentTickets;

    if (timeElapsed >= refreshInterval) {
      const ticketsToAdd = Math.floor(timeElapsed / refreshInterval);
      updatedTickets = Math.min(maxTickets, currentTickets + ticketsToAdd);
    }

    // Decrease the ticket count by 1 if it's greater than 0
    if (updatedTickets > 0) {
      updatedTickets--;
    } else {
      console.log("No tickets left to decrement.");
      throw new functions.https.HttpsError("failed-precondition"
          , "No tickets available to use.");
    }

    console.log("Updated Tickets after decrement:", updatedTickets);

    // Update the user's ticket count and the last refresh time
    await userRef.update({
      tickets: updatedTickets,
      lastRefresh: admin.firestore.Timestamp.now(),
    });

    return {tickets: updatedTickets};
  } catch (error) {
    console.error("Error in refreshTickets function:", error);
    throw new functions.https.HttpsError("unknown", error.message ||
          "An unknown error occurred.");
  }
});
