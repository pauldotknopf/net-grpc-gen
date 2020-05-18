#include "qtesttypes.h"
#include "qtesttypesworker.h"
#include <QThread>
#include <QMap>
#include <QJSValue>
#include <QSharedPointer>
#include <QQmlEngine>
#include <QQmlContext>
#include "roc-lib/qroccommon.h"
#include "private/qv4engine_p.h"
#include "private/qqmlengine_p.h"
using namespace Tests;
struct CallbackRequest
{
	QJSValue state;
	QJSValue callback;
};
class Tests::QTestTypesPrivate
{
public:
	QTestTypesPrivate() : currentRequestId(0) {}
	QThread workerThread;
	QTestTypesWorker* worker;
	QMap<int, QSharedPointer<CallbackRequest>> requests;
	int currentRequestId;
};
QTestTypes::QTestTypes(QObject* parent) : QObject(parent), d_priv(new Tests::QTestTypesPrivate())
{
	d_priv->worker = new QTestTypesWorker();
	d_priv->worker->moveToThread(&d_priv->workerThread);
	connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));
	d_priv->workerThread.start();
	connect(d_priv->worker, &QTestTypesWorker::testParamDoubleDone, this, &QTestTypes::testParamDoubleHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamFloatDone, this, &QTestTypes::testParamFloatHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamIntDone, this, &QTestTypes::testParamIntHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamUIntDone, this, &QTestTypes::testParamUIntHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamLongDone, this, &QTestTypes::testParamLongHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamULongDone, this, &QTestTypes::testParamULongHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamBoolDone, this, &QTestTypes::testParamBoolHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamStringDone, this, &QTestTypes::testParamStringHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamByteDone, this, &QTestTypes::testParamByteHandler);
	connect(d_priv->worker, &QTestTypesWorker::testParamBytesDone, this, &QTestTypes::testParamBytesHandler);
	connect(d_priv->worker, &QTestTypesWorker::testEventRaised, this, &QTestTypes::testEvent);
}
QTestTypes::~QTestTypes()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
void QTestTypes::testParamDouble(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamDouble(val, requestId);
}
void QTestTypes::testParamFloat(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamFloat(val, requestId);
}
void QTestTypes::testParamInt(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamInt(val, requestId);
}
void QTestTypes::testParamUInt(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamUInt(val, requestId);
}
void QTestTypes::testParamLong(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamLong(val, requestId);
}
void QTestTypes::testParamULong(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamULong(val, requestId);
}
void QTestTypes::testParamBool(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamBool(val, requestId);
}
void QTestTypes::testParamString(QVariant val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamString(val, requestId);
}
void QTestTypes::testParamByte(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamByte(val, requestId);
}
void QTestTypes::testParamBytes(QByteArray val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamBytes(val, requestId);
}
void QTestTypes::testParamDoubleHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamFloatHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamIntHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamUIntHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamLongHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamULongHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamBoolHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamStringHandler(QVariant val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
        //e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamByteHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamBytesHandler(QByteArray val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
        e.setProperty("state", request->state);
        auto execEngine = QQmlEnginePrivate::getV4Engine(engine);

        QV4::Scope scope(execEngine);
        QV4::ScopedValue v(scope, scope.engine->fromVariant(QVariant::fromValue(val)));
        //e.setProperty("result", v.js);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
