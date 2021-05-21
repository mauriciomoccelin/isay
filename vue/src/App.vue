<template>
  <v-app>
    <v-app-bar app>
      <v-toolbar-title>Signal R | Real Time Chat</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-switch class="pt-4" v-model="$vuetify.theme.dark"></v-switch>
    </v-app-bar>

    <v-main>
      <v-container class="fill-height pa-0">
        <v-row class="no-gutters elevation-4">
          <v-col cols="auto" class="flex-grow-1 flex-shrink-0">
            <v-responsive height="90vh" class="overflow-y-hidden fill-height">
              <v-card flat class="d-flex flex-column fill-height">
                <v-card-title> {{ model.user }} </v-card-title>
                <v-card-text class="flex-grow-1 overflow-y-auto">
                  <template v-for="(message, i) in messages">
                    <div
                      :class="{
                        'd-flex flex-row-reverse': model.user === message.user,
                      }"
                      :key="`Message:${i}`"
                    >
                      <v-chip
                        dark
                        class="pa-0 mb-2"
                        style="height: auto; white-space: normal"
                        :color="model.user === message.user ? 'primary' : ''"
                      >
                        <v-list-item>
                          <v-list-item-content>
                            <v-list-item-title class="text-wrap">
                              {{ message.text }}
                            </v-list-item-title>
                            <v-list-item-subtitle class="text-wrap">
                              {{ message.user }}
                            </v-list-item-subtitle>
                          </v-list-item-content>
                        </v-list-item>
                      </v-chip>
                    </div>
                  </template>
                </v-card-text>
                <v-card-text class="flex-shrink-1">
                  <v-text-field
                    shaped
                    outlined
                    no-details
                    type="text"
                    hide-details
                    @keyup.enter="send"
                    v-model="model.text"
                    @click:append-outer="send"
                    append-outer-icon="mdi-send"
                    label="Digite uma mensagem..."
                  />
                </v-card-text>
              </v-card>
            </v-responsive>
          </v-col>
        </v-row>
      </v-container>
    </v-main>

    <v-dialog v-model="dialog" persistent max-width="600px">
      <v-card>
        <v-card-title>
          <span class="headline">Usuário</span>
        </v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12">
              <v-text-field
                class="mt-8"
                label="Nome"
                v-model="model.user"
                hint="Informe seu nome completo ex: João da Silva"
              />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn
            text
            color="success"
            @click="dialog = false"
            :disabled="!hasUserName"
            >Começar</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script lang="ts">
import Vue from "vue";
import * as signalR from "@microsoft/signalr";
import { Component, Watch } from "vue-property-decorator";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://isay.azurewebsites.net/chathub")
  .withAutomaticReconnect()
  .build();

interface Message {
  user: string;
  text: string;
  sendOn: string;
}

@Component({
  filters: {
    date(value: Date) {
      return value ? new Date(value).toLocaleTimeString() : "";
    },
  },
})
export default class App extends Vue {
  name = "App";

  private dialog: boolean | null = false;
  private messages: Array<Message> = [];
  private model: Message = {
    user: "",
    text: "",
    sendOn: "",
  };

  get hasUserName(): boolean {
    const name = this.model.user;
    if (name === "") return false;
    if (name === "") return false;
    if (name === null) return false;
    return true;
  }

  received(message: Message): void {
    this.messages.push(message);
    this.messages = this.messages.sort((a, b) => {
      if (a.sendOn < b.sendOn) return -1;
      if (a.sendOn > b.sendOn) return 1;
      return 0;
    });
  }

  send(): void {
    this.model.sendOn = new Date().toJSON();

    connection.invoke("SendMessage", Object.assign({}, this.model));

    this.model.text = "";
    this.model.sendOn = "";
  }

  @Watch("model.user")
  onModelUserChanged(value: string): void {
    sessionStorage.setItem("user", value);
  }

  async mounted(): Promise<void> {
    const user = sessionStorage.getItem("user") as string;
    if (user === null) {
      this.dialog = true;
    } else {
      this.model.user = user;
    }

    await connection.start();
    connection.on("ReceiveMessage", this.received);

    fetch("https://isay.azurewebsites.net/messages/get-all")
      .then((response) => response.json())
      .then((messages) => (this.messages = messages as Array<Message>));
  }

  beforeDestroy(): void {
    connection.off("ReceiveMessage");
    connection.stop();
  }
}
</script>
