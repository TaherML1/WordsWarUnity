/* eslint-disable max-len */
/* eslint-disable no-unused-vars */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();
const database = admin.database();

exports.checkGameEnd = functions.database.ref("/games/{gameId}/gameEnd")
    .onCreate(async (snapshot, context) => {
      const gameId = context.params.gameId;

      try {
        // Retrieve game information
        const gameSnapshot = await admin.database().ref(`/games/${gameId}`).once("value");
        const game = gameSnapshot.val();

        // Extract player IDs and scores
        const player1Id = game.gameInfo.playersIds[0];
        const player2Id = game.gameInfo.playersIds[1];
        const player1Score = game.gameInfo.scores[player1Id];
        const player2Score = game.gameInfo.scores[player2Id];

        // Determine winner and loser
        let winner = "";
        let loser = "";
        if (player1Score > player2Score) {
          winner = player1Id;
          loser = player2Id;
        } else if (player1Score < player2Score) {
          winner = player2Id;
          loser = player1Id;
        } else {
          // If both players have the same score, determine the loser based on the current turn
          const currentTurn = game.turn;
          if (currentTurn === player1Id) {
            // Player 1 is the loser if it's their turn
            winner = player2Id;
            loser = player1Id;
          } else {
            // Player 2 is the loser if it's their turn
            winner = player1Id;
            loser = player2Id;
          }
        }

        // Update game state
        await admin.database().ref(`/games/${gameId}`).update({
          winner: winner,
          loser: loser,
          status: "ended", // You can update other game status as needed
        });

        // Calculate refresh time
        const refreshTime = admin.firestore.Timestamp.fromDate(new Date(Date.now() + 30 * 60 * 1000)); // 30 minutes from now

        // Update coins, other stats, and refresh time for the winner and loser in Firestore
        const winnerRef = admin.firestore().collection("users").doc(winner);
        const loserRef = admin.firestore().collection("users").doc(loser);

        await admin.firestore().runTransaction(async (transaction) => {
          const winnerDoc = await transaction.get(winnerRef);
          const loserDoc = await transaction.get(loserRef);

          const winnerCoins = winnerDoc.data().coins + 20; // Add 20 coins
          const winnerScores = winnerDoc.data().scores + 100;
          const winnerXP = winnerDoc.data().xp + 25; // Add 25 XP for the winner
          const winnerMatchesWon = winnerDoc.data().matchesWon + 1;
          const loserCoins = loserDoc.data().coins + 10; // Add 10 coins
          const loserXP = loserDoc.data().xp + 15; // Add 15 XP for the loser
          const loserScores = loserDoc.data().scores + 50;
          const loserMatchesLost = loserDoc.data().matchesLost + 1;

          // Decrement tickets for both players
          const winnerHintsRef = winnerRef.collection("hints").doc("hintsData");
          const loserHintsRef = loserRef.collection("hints").doc("hintsData");

          const winnerHintsDoc = await transaction.get(winnerHintsRef);
          const loserHintsDoc = await transaction.get(loserHintsRef);
          // Ensure tickets don't go below 0
          const winnerTickets = Math.max(winnerHintsDoc.data().tickets - 1, 0);
          const loserTickets = Math.max(loserHintsDoc.data().tickets - 1, 0);

          // Update Firestore documents with new values and set refreshTime
          transaction.update(winnerRef, {
            coins: winnerCoins,
            xp: winnerXP,
            scores: winnerScores,
            matchesWon: winnerMatchesWon,
            refreshTime: refreshTime, // Set refresh time for the winner
          });

          transaction.update(loserRef, {
            coins: loserCoins,
            xp: loserXP,
            scores: loserScores,
            matchesLost: loserMatchesLost,
            refreshTime: refreshTime, // Set refresh time for the loser
          });

          transaction.update(winnerHintsRef, {tickets: winnerTickets});
          transaction.update(loserHintsRef, {tickets: loserTickets});

          console.log("Updated winner's and loser's stats and set refresh time.");
        });

        console.log("Game ended:", gameId);
      } catch (error) {
        console.error("Error:", error);
      }
    });
