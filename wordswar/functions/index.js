/* eslint-disable require-jsdoc */
/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();
const database = admin.database();

exports.startGameOnInvitationAccept = functions.database.ref("invitations/{invitationId}/status")
    .onUpdate((change, context) => {
      const status = change.after.val();
      if (status === "accepted") {
        const invitationId = context.params.invitationId;

        return database.ref(`invitations/${invitationId}`).once("value").then((snapshot) => {
          const invitation = snapshot.val();
          const fromPlayerId = invitation.from;
          const toPlayerId = invitation.to;
          const gameId = generateGameId();

          const game = {
            gameInfo: {
              gameId: gameId,
              playersIds: [fromPlayerId, toPlayerId],
              scores: {
                [fromPlayerId]: 0,
                [toPlayerId]: 0,
              },
              usedwords: {
                [fromPlayerId]: [""],
                [toPlayerId]: [""],
              },
              timer: 15,
            },
            turn: fromPlayerId,
          };

          return database.ref("games/" + gameId).set(game).then(() => {
            console.log("Game created successfully!");
            return null;
          });
        });
      }
      return null;
    });

function generateGameId() {
  return "game-" + Math.random().toString(36).substr(2, 9);
}
