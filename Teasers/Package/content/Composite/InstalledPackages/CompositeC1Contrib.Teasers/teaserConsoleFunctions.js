var setFocus = function (a) {
    a = $(a);

    var token = a.data('token');

    EventBroadcaster.broadcast(BroadcastMessages.SYSTEMTREEBINDING_FOCUS, token);
};

var executeAction = function (a) {
    a = $(a);

    var providerName = a.data('providername');
    var entityToken = a.data('entitytoken');
    var actionToken = a.data('actiontoken');
    var piggybag = a.data('piggybag');
    var piggybagHash = a.data('piggybaghash');

    var clientElement = new ClientElement(providerName, entityToken, piggybag, piggybagHash);
    var actionElement = new ActionElement(a.html(), actionToken);

    var systemAction = new SystemAction(actionElement);
    var systemNode = new SystemNode(clientElement);

    SystemAction.invoke(systemAction, systemNode);
};

function ClientElement(providerName, entityToken, piggybag, piggybagHash) {
    this.ProviderName = providerName;
    this.EntityToken = entityToken;
    this.Piggybag = piggybag;
    this.PiggybagHash = piggybagHash;
    this.HasChildren = false;
    this.IsDisabled = false;
    this.DetailedDropSupported = false;
    this.ContainsTaggedActions = false;
    this.TreeLockEnabled = false;

    return this;
};

function ActionElement(label, actionToken) {
    this.Label = label;
    this.ActionToken = actionToken;

    return this;
};