#include "qtesttypes.h"
#include "qtesttypesworker.h"
#include <QThread>
#include <QMap>
#include <QJSValue>
#include <QSharedPointer>
#include <QQmlEngine>
#include <QQmlContext>
#include "roc-lib/qroccommon.h"
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
}
QTestTypes::~QTestTypes()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
void QTestTypes::testParamDouble(double val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamDouble(val, requestId);
}
void QTestTypes::testParamFloat(float val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamFloat(val, requestId);
}
void QTestTypes::testParamInt(int val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamInt(val, requestId);
}
void QTestTypes::testParamUInt(quint32 val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamUInt(val, requestId);
}
void QTestTypes::testParamLong(qint64 val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamLong(val, requestId);
}
void QTestTypes::testParamULong(ulong val, QJSValue state, QJSValue callback)
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
void QTestTypes::testParamString(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testParamString(val, requestId);
}
void QTestTypes::testParamByte(quint32 val, QJSValue state, QJSValue callback)
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
void QTestTypes::testParamDoubleHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamFloatHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamIntHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamUIntHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamLongHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamULongHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamBoolHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamStringHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamByteHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTestTypes::testParamBytesHandler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
